using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class PcBuild_Ram
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public Guid RamId { get; set; }
        [ForeignKey("RamId")]
        public Ram Ram { get; set; } = null!;

        public Guid? ProductOfferId { get; set; }
        [ForeignKey("ProductOfferId")]
        public ProductOffer? ProductOffer { get; set; }

        [Required]
        public Guid PcBuildId { get; set; }
        [ForeignKey("PcBuildId")]
        public PcBuild PcBuild { get; set; } = null!;
    }
}
