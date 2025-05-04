using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class PcCaseFormFactor
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid PcCaseId { get; set; }

        [ForeignKey("PcCaseId")]
        public PcCase PcCase { get; set; } = null!;

    }
}
