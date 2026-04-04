namespace PcBuilder.Api.Services
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync<T>(string queueName, T message, CancellationToken ct = default);
    }
}
