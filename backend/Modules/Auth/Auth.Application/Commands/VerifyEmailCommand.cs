using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record VerifyEmailCommand(string Token) : IRequest<Result<string>>;
}
