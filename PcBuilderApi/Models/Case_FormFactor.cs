using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class Case_FormFactor
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CaseId { get; set; }

        [ForeignKey("CaseId")]
        public Case Case { get; set; } = null!;

        [Required]
        public Guid FormFactorId { get; set; }

        [ForeignKey("FormFactorId")]
        public FormFactor FormFactor { get; set; } = null!;
    }
}
