using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class GpuPowerConnector
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Pins { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public Guid GpuId { get; set; }

        [ForeignKey("GpuId")]
        public Gpu Gpu { get; set; } = null!;
    }
}
