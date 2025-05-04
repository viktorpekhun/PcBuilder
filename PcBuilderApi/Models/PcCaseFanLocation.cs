using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class PcCaseFanLocation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

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
