using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class M2Slot
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public double Version { get; set; }
        public int? Lane { get; set; }
        public int? Quantity { get; set; }


        [Required]
        public Guid MotherboardId { get; set; }

        [ForeignKey("MotherboardId")]
        public Motherboard Motherboard { get; set; } = null!;
    }
}
