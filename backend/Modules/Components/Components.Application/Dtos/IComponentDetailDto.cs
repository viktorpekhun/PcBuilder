namespace Components.Application.Dtos
{
    public interface IComponentDetailDto : IHasProductOffers
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
