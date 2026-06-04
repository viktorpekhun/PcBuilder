namespace Moderation.Application.Dtos
{
    public class AdminActivityLogDto
    {
        public Guid Id { get; set; }
        public string AdminUsername { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? TargetType { get; set; }
        public Guid? TargetId { get; set; }
        public string? TargetName { get; set; }
        public string? Detail { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
