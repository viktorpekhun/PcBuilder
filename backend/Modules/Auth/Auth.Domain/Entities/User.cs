using System.ComponentModel.DataAnnotations;

namespace Auth.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(50, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(30, ErrorMessage = "Username can't have more than 30 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "Username can only contain letters, numbers, underscores (_) and hyphens (-)")]
        public string Username { get; set; } = string.Empty;

        public string? PasswordHash { get; set; }

        public string? GoogleId { get; set; }

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }

        public DateTime CommentBanUntil { get; set; }
        public DateTime PostBanUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.Now;

        public ICollection<Role> Roles { get; set; } = [];
    }
}
