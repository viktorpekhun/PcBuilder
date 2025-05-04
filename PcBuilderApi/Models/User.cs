using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(50, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(30, ErrorMessage = "Username can't have more than 30 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "Username can only contain letters, numbers, underscores (_) and hyphens (-)")]
        public string Username { get; set; } = string.Empty;

        //[RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Password must contain at least one letter and one number.")]
        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CommentBanUntil { get; set; }
        public DateTime PostBanUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.Now;

        public List<PcBuild> PcBuilds { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
