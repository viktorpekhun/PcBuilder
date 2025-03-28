using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class Case_FanLocation
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public int FanSize { get; set; }
        [Required]
        public int MaxFans { get; set; }


        [Required]
        public Guid CaseId { get; set; }

        [ForeignKey("CaseId")]
        public Case Case { get; set; } = null!;

        [Required]
        public Guid FanLocationId { get; set; }

        [ForeignKey("FanLocationId")]
        public FanLocation FanLocation { get; set; } = null!;

    }
}
