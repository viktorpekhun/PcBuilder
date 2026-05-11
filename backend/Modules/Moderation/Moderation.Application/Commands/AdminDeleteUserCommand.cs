using MediatR;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record AdminDeleteUserCommand(Guid AdminId, Guid TargetUserId) : IRequest<Result<bool>>;
}