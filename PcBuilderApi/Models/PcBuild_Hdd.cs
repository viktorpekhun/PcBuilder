using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class PcBuild_Hdd
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public Guid HddId { get; set; }
        [ForeignKey("HddId")]
        public Hdd Hdd { get; set; } = null!;

        [Required]
        public Guid PcBuildId { get; set; }
        [ForeignKey("PcBuildId")]
        public PcBuild PcBuild { get; set; } = null!;
    }
}
