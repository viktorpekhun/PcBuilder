namespace Components.Application.Dtos
{
    public record PriceHistoryPointDto(
        DateOnly Date,
        decimal AveragePrice,
        int OffersCount);
}
