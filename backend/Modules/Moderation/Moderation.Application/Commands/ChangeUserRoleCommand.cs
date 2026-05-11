using MediatR;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Commands
{
    public record ChangeUserRoleCommand(
        Guid AdminId,
        Guid UserId,
        string Role) : IRequest<Result<bool>>;
}