using System.ComponentModel.DataAnnotations;
using Auth.Domain.Entities;

namespace PcBuilds.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }

        [MaxLength(500, ErrorMessage = "Comment text can't have more than 500 characters.")]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid PcBuildId { get; set; }
        public PcBuild PcBuild { get; set; } = null!;
    }
}