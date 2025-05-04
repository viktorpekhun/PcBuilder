using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class RearPort
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        public int? Quantity { get; set; }

        [Required]
        public Guid MotherboardId { get; set; }

        [ForeignKey("MotherboardId")]
        public Motherboard Motherboard { get; set; } = null!;
    }
}
