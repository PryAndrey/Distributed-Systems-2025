using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Valuator.Services;

public interface IMessageQueueService
{
    Task SendIdMessageAsync(string queueName, string message);
    Task SendEventAsync(string textId, string eventType, double value);
}

public class MessageQueue(IConnection rabbitMqConnection) : IMessageQueueService
{
    public async Task SendIdMessageAsync(string queueName, string id)
    {
        Console.WriteLine($"Sending id: {id}");
        await using var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(id);
        await channel.BasicPublishAsync("", routingKey: queueName, body);

        await Task.CompletedTask;
    }

    public async Task SendEventAsync(string textId, string eventType, double value)
    {
        await using var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("events_exchange", ExchangeType.Fanout, true);

        var eventData = new { EventType = eventType, TextId = textId, Similarity = value };
        var eventJson = JsonSerializer.Serialize(eventData);
        var body = Encoding.UTF8.GetBytes(eventJson);

        await channel.BasicPublishAsync("events_exchange", "", body);
    }
}