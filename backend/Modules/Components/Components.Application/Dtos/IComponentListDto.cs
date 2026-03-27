namespace Components.Application.Dtos
{
    public interface IComponentListDto
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string? PhotoUrl { get; set; }
        string Brand { get; set; }
        decimal? AveragePrice { get; set; }
        int? OffersCount { get; set; }
    }
}
