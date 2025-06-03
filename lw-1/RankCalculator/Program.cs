using System.Globalization;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace RankCalculator;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var redis = await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("DB_MAIN"));
            var db = redis.GetDatabase();

            var regionalRedisConnections = new Dictionary<string, IConnectionMultiplexer>();
            regionalRedisConnections["RU"] =
                await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("DB_RU"));
            regionalRedisConnections["EU"] =
                await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("DB_EU"));
            regionalRedisConnections["ASIA"] =
                await ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable("DB_ASIA"));

            var regionalDbs = new Dictionary<string, IDatabase>();
            regionalDbs["RU"] = regionalRedisConnections["RU"].GetDatabase();
            regionalDbs["EU"] = regionalRedisConnections["EU"].GetDatabase();
            regionalDbs["ASIA"] = regionalRedisConnections["ASIA"].GetDatabase();

            var centrifugoService = new CentrifugoModule();

            var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "appuser";
            var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "1234";
            var factory = new ConnectionFactory { HostName = "rabbitmq", UserName = rabbitUser, Password = rabbitPass };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync("text_queue", true, false, false);

            await channel.ExchangeDeclareAsync("events_exchange", ExchangeType.Fanout, true);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                var id = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                Console.WriteLine($"LOOKUP: {id}, MAIN");
                var regionValue = await db.StringGetAsync($"REGION-{id}");

                if (!regionValue.HasValue) return;

                var region = regionValue.ToString();

                if (!regionalDbs.TryGetValue(region, out var regionalDb)) return;

                Console.WriteLine($"LOOKUP: {id}, {region}");
                var text = await regionalDb.StringGetAsync("TEXT-" + id);

                if (!text.HasValue) return;

                var textStr = text.ToString();

                var rank = CalculateRank(textStr);

                Console.WriteLine($"LOOKUP: {id}, {region}");
                // Искусственная задержка
                // await Task.Delay(3000);

                await regionalDb.StringSetAsync("RANK-" + id, rank);

                // Console.WriteLine("Send centrifugo");
                // await centrifugoService.PublishAsync($"text:{id}", rank.ToString(CultureInfo.InvariantCulture));

                await channel.BasicPublishAsync("events_exchange", "", CreateMessageBody(id, rank));
            };
            await channel.BasicConsumeAsync("text_queue", true, consumer);

            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex)
        {
            Console.ReadKey();
        }
    }

    static double CalculateRank(string text)
    {
        if (String.IsNullOrEmpty(text)) return 0;

        double count = 0;
        foreach (var character in text)
        {
            count += Char.IsLetter(char.ToLower(character)) ? 1 : 0;
        }

        var result = 1 - count / text.Length;

        return result;
    }

    static byte[] CreateMessageBody(string id, double value)
    {
        var eventData = new { EventType = "RankCalculated", TextId = id, Rank = value };
        var eventJson = JsonSerializer.Serialize(eventData);
        return Encoding.UTF8.GetBytes(eventJson);
    }
}