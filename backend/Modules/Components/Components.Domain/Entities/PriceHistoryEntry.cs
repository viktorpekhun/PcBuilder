using PcBuilder.SharedKernel.Enums;

namespace Components.Domain.Entities
{
    public class PriceHistoryEntry
    {
        public Guid Id { get; set; }

        public Guid ComponentId { get; set; }

        public ComponentType ComponentType { get; set; }

        public decimal AveragePrice { get; set; }

        public int OffersCount { get; set; }

        public DateOnly SnapshotDate { get; set; }
    }
}
