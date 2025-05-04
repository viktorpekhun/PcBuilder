using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Range(0, 5, ErrorMessage = "Rating must be between 0.00 and 5.00.")]
        public decimal Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Review text can't have more than 500 characters.")]
        public string? Text { get; set; }

        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public Guid PcBuildId { get; set; }
        [ForeignKey("PcBuildId")]
        public PcBuild PcBuild { get; set; } = null!;
    }
}
