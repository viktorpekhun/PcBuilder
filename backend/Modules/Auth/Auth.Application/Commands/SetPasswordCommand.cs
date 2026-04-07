using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record SetPasswordCommand(Guid UserId, string NewPassword) : IRequest<Result<string>>;
}
