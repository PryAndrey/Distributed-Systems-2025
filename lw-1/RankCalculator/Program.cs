using System.Text;
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
            var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379,abortConnect=false");
            var db = redis.GetDatabase();
            
            var factory = new ConnectionFactory { HostName = "localhost" };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            
            await channel.QueueDeclareAsync("text_queue", true, false, false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                
                var text = await db.StringGetAsync("TEXT-" + message);
                
                if (!text.HasValue)
                {
                    return;
                }

                var textStr = text.ToString();

                var rank = CalculateRank(textStr);
                await db.StringSetAsync("RANK-" + message, rank);
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
}