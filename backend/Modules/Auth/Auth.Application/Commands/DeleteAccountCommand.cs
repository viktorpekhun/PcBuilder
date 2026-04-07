using MediatR;
using PcBuilder.SharedKernel;

namespace Auth.Application.Commands
{
    public record DeleteAccountCommand(Guid UserId) : IRequest<Result<string>>;
}
