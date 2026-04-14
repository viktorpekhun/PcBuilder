namespace Moderation.Application.Dtos
{
    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public List<string> Roles { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime? CommentBanUntil { get; set; }
        public DateTime? PostBanUntil { get; set; }
        public bool IsCommentBanned { get; set; }
        public bool IsPostBanned { get; set; }
        public int WarningsCount { get; set; }
    }
}
