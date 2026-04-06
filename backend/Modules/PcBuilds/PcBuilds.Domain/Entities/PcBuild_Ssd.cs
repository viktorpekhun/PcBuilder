using Components.Domain.Entities;

namespace PcBuilds.Domain.Entities
{
    public class PcBuild_Ssd
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; } = 1;

        public Guid SsdId { get; set; }
        public Ssd Ssd { get; set; } = null!;

        public Guid? ProductOfferId { get; set; }
        public ProductOffer? ProductOffer { get; set; }

        public Guid PcBuildId { get; set; }
        public PcBuild PcBuild { get; set; } = null!;
    }
}
