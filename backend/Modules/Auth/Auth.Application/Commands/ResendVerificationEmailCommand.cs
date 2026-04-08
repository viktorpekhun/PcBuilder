using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record ResendVerificationEmailCommand(Guid UserId) : IRequest<Result<string>>;
}
