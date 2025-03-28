using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class CpuPowerConnector
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public int Pins { get; set; }

        [Required]
        public int Quantity { get; set; }



        [Required]
        public Guid MotherboardId { get; set; }

        [ForeignKey("MotherboardId")]
        public Motherboard Motherboard { get; set; } = null!;
    }
}
