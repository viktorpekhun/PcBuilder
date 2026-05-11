namespace PcBuilds.Application.Dtos
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}