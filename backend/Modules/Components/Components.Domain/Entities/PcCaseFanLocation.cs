using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Components.Domain.ValueObjects;

namespace Components.Domain.Entities
{
    public class PcCaseFanLocation
    {
        [Key]
        public Guid Id { get; set; }

        public LocalizedString Name { get; set; } = new();

        [Required]
        public int FanSize { get; set; }
        [Required]
        public int MaxFans { get; set; }

        [Required]
        public Guid PcCaseId { get; set; }

        [ForeignKey("PcCaseId")]
        public PcCase PcCase { get; set; } = null!;
    }
}
