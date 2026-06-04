using PcBuilder.SharedKernel.Enums;

namespace Components.Domain.Entities
{
    public class ProductOffer
    {
        public Guid Id { get; set; }

        public decimal Price { get; set; }

        public ComponentType ComponentType { get; set; }

        public Guid ComponentId { get; set; }

        public string ProductOfferUrl { get; set; } = string.Empty;

        public Guid StoreId { get; set; }

        public Store Store { get; set; } = null!;

        // "in" | "low" | "out" — populated by scraper; null until next scrape run
        public string? StockStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
