using PcBuilder.SharedKernel.Enums;

namespace Components.Domain.Entities
{
    public class PriceAlertSubscription
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ComponentId { get; set; }

        public ComponentType ComponentType { get; set; }

        public decimal ThresholdPercent { get; set; }

        public decimal InitialPrice { get; set; }

        public decimal LastNotifiedPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
