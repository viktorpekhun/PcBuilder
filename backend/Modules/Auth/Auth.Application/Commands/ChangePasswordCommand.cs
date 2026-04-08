using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result<string>>;
}
