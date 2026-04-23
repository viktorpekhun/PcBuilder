using Microsoft.AspNetCore.SignalR;
using Notifications.Application.Dtos;
using PcBuilder.Api.Hubs;
using PcBuilder.Contracts.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PcBuilder.Api.Services
{
    public class SignalRNotificationConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<SignalRNotificationConsumer> _logger;

        public SignalRNotificationConsumer(
            IConnection connection,
            IHubContext<NotificationHub> hub,
            ILogger<SignalRNotificationConsumer> logger)
        {
            _connection = connection;
            _hub = hub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync("signalr-notifications", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<SignalRNotificationMessage>(json);

                    if (message != null)
                    {
                        Dictionary<string, string> payload;
                        try
                        {
                            payload = JsonSerializer.Deserialize<Dictionary<string, string>>(message.Payload)
                                      ?? new Dictionary<string, string>();
                        }
                        catch
                        {
                            payload = new Dictionary<string, string>();
                        }

                        var dto = new NotificationDto
                        {
                            Id = message.NotificationId,
                            Type = message.Type,
                            Payload = payload,
                            IsRead = false,
                            CreatedAt = message.CreatedAt
                        };

                        await _hub.Clients
                            .Group($"user-{message.UserId}")
                            .SendAsync("notification", dto, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error pushing SignalR notification from queue");
                }
                finally
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
            };

            await channel.BasicConsumeAsync("signalr-notifications", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
        }
    }
}
