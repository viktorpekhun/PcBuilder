using MediatR;
using PcBuilder.SharedKernel;

namespace Notifications.Application.Commands
{
    public record CreateNotificationCommand(
        Guid UserId,
        string Type,
        Dictionary<string, string> Payload) : IRequest<Result<Guid>>;
}