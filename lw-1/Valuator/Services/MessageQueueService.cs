using System.Text;
using RabbitMQ.Client;

namespace Valuator.Services;

public interface IMessageQueueService
{
    Task PublishMessageAsync(string queueName, string message);
}

public class MessageQueueService(IConnection rabbitMqConnection) : IMessageQueueService
{
    public async Task PublishMessageAsync(string queueName, string message)
    {
        await using var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync("", routingKey: queueName, body);

        await Task.CompletedTask;
    }
}