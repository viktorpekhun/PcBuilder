using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Dtos
{
    public record PriceAlertDto(
        Guid Id,
        Guid ComponentId,
        ComponentType ComponentType,
        decimal ThresholdPercent,
        decimal LastNotifiedPrice,
        DateTime CreatedAt);
}
