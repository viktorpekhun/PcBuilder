using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class FormFactor
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; } = string.Empty;

        public List<Case_FormFactor> Case_FormFactors { get; set; } = new();
    }
}
