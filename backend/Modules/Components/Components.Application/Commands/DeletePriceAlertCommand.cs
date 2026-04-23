using MediatR;
using PcBuilder.SharedKernel;

namespace Components.Application.Commands
{
    public record DeletePriceAlertCommand(
        Guid UserId,
        Guid Id) : IRequest<Result<bool>>;
}
