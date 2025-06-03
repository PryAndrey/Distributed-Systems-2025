using System.Globalization;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace RankCalculator;

public class RankCalculator
{
    public static double CalculateRank(string text)
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
}

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379,abortConnect=false");
            var db = redis.GetDatabase();
            
            var centrifugoService = new CentrifugoModule();
            var factory = new ConnectionFactory { HostName = "localhost" };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            await channel.QueueDeclareAsync("text_queue", true, false, false);

            await channel.ExchangeDeclareAsync("events_exchange", ExchangeType.Fanout, true);
            
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                var id = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                
                var text = await db.StringGetAsync("TEXT-" + id);
                
                if (!text.HasValue)
                    return;

                var textStr = text.ToString();

<<<<<<< Updated upstream
                var rank = CalculateRank(textStr);
                
=======
                var rank = RankCalculator.CalculateRank(textStr);

                Console.WriteLine($"LOOKUP: {id}, {region}");
>>>>>>> Stashed changes
                // Искусственная задержка
                await Task.Delay(3000);
                
                await db.StringSetAsync("RANK-" + id, rank);
                
                
                await centrifugoService.PublishAsync($"text:{id}", rank.ToString(CultureInfo.InvariantCulture));
                
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

<<<<<<< Updated upstream
    static double CalculateRank(string text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return 0;
        }
        
        double count = 0;
        foreach (var character in text)
        {
            count += Char.IsLetter(char.ToLower(character)) ? 1 : 0;
        }

        var result = 1 - count / text.Length;
        
        return result;
    }   
    // todo начало с глагола
    // todo переименовать на message
=======


>>>>>>> Stashed changes
    static byte[] CreateMessageBody(string id, double value)
    {
        var eventData = new { EventType = "RankCalculated", TextId = id, Rank = value };
        var eventJson = JsonSerializer.Serialize(eventData);
        return Encoding.UTF8.GetBytes(eventJson);
    }
}