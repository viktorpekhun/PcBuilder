using Components.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Queries
{
    public record GetPriceAlertForComponentQuery(
        Guid UserId,
        Guid ComponentId,
        ComponentType ComponentType) : IRequest<Result<PriceAlertDto?>>;
}
