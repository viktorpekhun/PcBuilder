namespace Auth.Application.Dtos
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public bool HasPassword { get; set; }
        public bool GoogleLinked { get; set; }
        public List<string> Roles { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public int BuildCount { get; set; }
    }
}
