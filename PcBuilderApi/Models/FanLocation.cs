using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class FanLocation
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public List<Case_FanLocation> Case_FanLocations { get; set; } = new();
    }
}
