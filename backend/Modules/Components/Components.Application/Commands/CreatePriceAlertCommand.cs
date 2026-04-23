using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Commands
{
    public record CreatePriceAlertCommand(
        Guid UserId,
        Guid ComponentId,
        ComponentType ComponentType,
        decimal ThresholdPercent) : IRequest<Result<Guid>>;
}
