namespace Components.Domain.Entities
{
    public class Store
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public List<ProductOffer> ProductOffers { get; set; } = new();
    }
}
