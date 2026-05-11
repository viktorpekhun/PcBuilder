using MediatR;

namespace Notifications.Application.Events
{
    public record NotificationCreatedEvent(
        Guid NotificationId,
        Guid UserId,
        string Type,
        string Payload,
        DateTime CreatedAt) : INotification;
}
