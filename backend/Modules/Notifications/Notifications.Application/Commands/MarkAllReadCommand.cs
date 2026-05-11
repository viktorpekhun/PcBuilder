using MediatR;
using PcBuilder.SharedKernel;

namespace Notifications.Application.Commands
{
    public record MarkAllReadCommand(Guid UserId) : IRequest<Result<int>>;
}