using System.ComponentModel.DataAnnotations;
using Auth.Domain.Entities;
using Moderation.Domain.Enums;

namespace Moderation.Domain.Entities
{
    public class Warning
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public BanType BanType { get; set; }

        [MaxLength(500, ErrorMessage = "Reason can't have more than 500 characters.")]
        public string Reason { get; set; } = string.Empty;

        public Guid? IssuedByAdminId { get; set; }
        public User? IssuedByAdmin { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
