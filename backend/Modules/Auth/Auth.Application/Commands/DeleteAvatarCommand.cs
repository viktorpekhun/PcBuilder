using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record DeleteAvatarCommand(Guid UserId) : IRequest<Result<string>>;
}
