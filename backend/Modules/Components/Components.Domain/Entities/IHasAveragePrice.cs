namespace Components.Domain.Entities
{
    public interface IHasAveragePrice
    {
        Guid Id { get; set; }
        decimal? AveragePrice { get; set; }
    }
}
