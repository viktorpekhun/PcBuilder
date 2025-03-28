using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class PcleSlot
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
