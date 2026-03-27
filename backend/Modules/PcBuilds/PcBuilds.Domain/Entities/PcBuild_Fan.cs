using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Components.Domain.Entities;

namespace PcBuilds.Domain.Entities
{
    public class PcBuild_Fan
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public Guid FanId { get; set; }
        [ForeignKey("FanId")]
        public Fan Fan { get; set; } = null!;

        public Guid? ProductOfferId { get; set; }
        [ForeignKey("ProductOfferId")]
        public ProductOffer? ProductOffer { get; set; }

        [Required]
        public Guid PcBuildId { get; set; }
        [ForeignKey("PcBuildId")]
        public PcBuild PcBuild { get; set; } = null!;
    }
}
