using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventsLogger;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("events_exchange", ExchangeType.Fanout, true);

        var queueName = $"events_queue_{Guid.NewGuid()}";

        await channel.QueueDeclareAsync(queueName, true, false, true);

        await channel.QueueBindAsync(queueName, "events_exchange", "");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            try
            {
                var eventData = JsonSerializer.Deserialize<EventData>(message);
                PrintEvents(eventData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }

            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueName, true, consumer);

        Console.WriteLine("EventsLogger started");
        await Task.Delay(Timeout.Infinite);
    }

    static void PrintEvents(EventData? eventData)
    {
        if (eventData != null)
        {
            switch (eventData.EventType)
            {
                case "RankCalculated":
                    Console.WriteLine($"Event: {eventData.EventType}: {eventData.TextId}");
                    Console.WriteLine($"Rank: {eventData.Rank}");
                    break;
                case "SimilarityCalculated":
                    Console.WriteLine($"Event: {eventData.EventType}: {eventData.TextId}");
                    Console.WriteLine($"Similarity: {eventData.Similarity}");
                    break;
                default:
                    Console.WriteLine($"Unknown event: {eventData.EventType}");
                    break;
            }
        }
    }
}