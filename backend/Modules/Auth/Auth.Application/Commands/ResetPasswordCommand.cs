using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<Result<string>>;
}
