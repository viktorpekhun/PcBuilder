using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record ForgotPasswordCommand(string Email) : IRequest<Result<string>>;
}
