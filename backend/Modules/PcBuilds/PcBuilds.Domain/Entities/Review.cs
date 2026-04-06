using System.ComponentModel.DataAnnotations;
using Auth.Domain.Entities;

namespace PcBuilds.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0.00 and 5.00.")]
        public decimal Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Review text can't have more than 500 characters.")]
        public string? Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid PcBuildId { get; set; }
        public PcBuild PcBuild { get; set; } = null!;
    }
}
