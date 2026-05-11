using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Dtos
{
    public record UserPriceAlertDto(
        Guid Id,
        Guid ComponentId,
        ComponentType ComponentType,
        string? ComponentName,
        string? ComponentImageUrl,
        decimal ThresholdPercent,
        decimal InitialPrice,
        decimal LastNotifiedPrice,
        decimal? CurrentAveragePrice,
        DateTime CreatedAt);
}
