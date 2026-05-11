using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Notifications.Application.Dtos;
using Notifications.Application.Events;
using PcBuilder.Api.Hubs;

namespace PcBuilder.Api.RealTime
{
    public class NotificationSignalRPublisher : INotificationHandler<NotificationCreatedEvent>
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationSignalRPublisher(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task Handle(NotificationCreatedEvent notification, CancellationToken cancellationToken)
        {
            Dictionary<string, string> payload;
            try
            {
                payload = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.Payload)
                          ?? new Dictionary<string, string>();
            }
            catch
            {
                payload = new Dictionary<string, string>();
            }

            var dto = new NotificationDto
            {
                Id = notification.NotificationId,
                Type = notification.Type,
                Payload = payload,
                IsRead = false,
                CreatedAt = notification.CreatedAt
            };

            await _hub.Clients
                .Group($"user-{notification.UserId}")
                .SendAsync("notification", dto, cancellationToken);
        }
    }
}
