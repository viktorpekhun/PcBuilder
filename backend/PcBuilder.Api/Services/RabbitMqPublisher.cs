using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PcBuilder.Api.Services
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
    {
        private readonly IConnection _connection;
        private IChannel? _channel;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public RabbitMqPublisher(IConnection connection)
        {
            _connection = connection;
        }

        private async Task<IChannel> GetChannelAsync(CancellationToken ct)
        {
            if (_channel != null)
                return _channel;

            await _initLock.WaitAsync(ct);
            try
            {
                _channel ??= await _connection.CreateChannelAsync(cancellationToken: ct);
            }
            finally
            {
                _initLock.Release();
            }

            return _channel;
        }

        public async Task PublishAsync<T>(string queueName, T message, CancellationToken ct = default)
        {
            var channel = await GetChannelAsync(ct);
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var props = new BasicProperties { Persistent = true };

            await channel.BasicPublishAsync("", queueName, mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
                await _channel.DisposeAsync();
        }
    }
}
