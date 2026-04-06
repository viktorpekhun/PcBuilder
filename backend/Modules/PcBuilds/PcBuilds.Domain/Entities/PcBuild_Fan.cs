using Components.Domain.Entities;

namespace PcBuilds.Domain.Entities
{
    public class PcBuild_Fan
    {
        public Guid Id { get; set; }

        public int Quantity { get; set; } = 1;

        public Guid FanId { get; set; }
        public Fan Fan { get; set; } = null!;

        public Guid? ProductOfferId { get; set; }
        public ProductOffer? ProductOffer { get; set; }

        public Guid PcBuildId { get; set; }
        public PcBuild PcBuild { get; set; } = null!;
    }
}
