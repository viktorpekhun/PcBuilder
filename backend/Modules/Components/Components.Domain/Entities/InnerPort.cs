using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class InnerPort
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = string.Empty;

        [Required]
        public Guid MotherboardId { get; set; }

        [ForeignKey("MotherboardId")]
        public Motherboard Motherboard { get; set; } = null!;
    }
}
