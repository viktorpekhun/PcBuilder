using MediatR;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record AdminDeleteBuildCommand(Guid AdminId, Guid BuildId, bool NotifyUser = true) : IRequest<Result<bool>>;
}