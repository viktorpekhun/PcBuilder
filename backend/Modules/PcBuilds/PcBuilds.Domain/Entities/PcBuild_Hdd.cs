using Components.Domain.Entities;

namespace PcBuilds.Domain.Entities
{
    public class PcBuild_Hdd
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; } = 1;

        public Guid HddId { get; set; }
        public Hdd Hdd { get; set; } = null!;

        public Guid? ProductOfferId { get; set; }
        public ProductOffer? ProductOffer { get; set; }

        public Guid PcBuildId { get; set; }
        public PcBuild PcBuild { get; set; } = null!;
    }
}
