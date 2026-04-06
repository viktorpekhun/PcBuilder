using Components.Domain.ValueObjects;

namespace Components.Domain.Entities
{
    public class PcCaseFanLocation
    {
        public Guid Id { get; set; }

        public LocalizedString Name { get; set; } = new();

        public int FanSize { get; set; }
        public int MaxFans { get; set; }

        public Guid PcCaseId { get; set; }

        public PcCase PcCase { get; set; } = null!;
    }
}
