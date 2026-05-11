using MediatR;
using PcBuilder.SharedKernel;

namespace Notifications.Application.Commands
{
    public record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : IRequest<Result<bool>>;
}