using Components.Domain.Entities;

namespace PcBuilds.Domain.Entities
{
    public class PcBuild_Ram
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; } = 1;

        public Guid RamId { get; set; }
        public Ram Ram { get; set; } = null!;

        public Guid? ProductOfferId { get; set; }
        public ProductOffer? ProductOffer { get; set; }

        public Guid PcBuildId { get; set; }
        public PcBuild PcBuild { get; set; } = null!;
    }
}
