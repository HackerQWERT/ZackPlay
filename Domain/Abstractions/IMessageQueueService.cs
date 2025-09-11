namespace Domain.Abstractions;

public interface IMessageQueueService
{
    Task PublishAsync<T>(string queueName, T message);
    Task SubscribeAsync<T>(string queueName, Func<T, Task> handler);
}
