using MediatR;
using Notifications.Application.Events;
using PcBuilder.Contracts.Messages;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PcBuilder.ScraperWorker
{
    public class RabbitMqNotificationPublisher : INotificationHandler<NotificationCreatedEvent>
    {
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMqNotificationPublisher> _logger;

        public RabbitMqNotificationPublisher(IConnection connection, ILogger<RabbitMqNotificationPublisher> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task Handle(NotificationCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                await channel.QueueDeclareAsync("signalr-notifications", durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

                var message = new SignalRNotificationMessage(
                    notification.NotificationId,
                    notification.UserId,
                    notification.Type,
                    notification.Payload,
                    notification.CreatedAt);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var props = new BasicProperties { Persistent = true };

                await channel.BasicPublishAsync("", "signalr-notifications", mandatory: false, basicProperties: props, body: body, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish SignalR notification {NotificationId} to RabbitMQ", notification.NotificationId);
            }
        }
    }
}
