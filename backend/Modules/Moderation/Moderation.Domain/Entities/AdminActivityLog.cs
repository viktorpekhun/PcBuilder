using Auth.Domain.Entities;

namespace Moderation.Domain.Entities
{
    public class AdminActivityLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AdminId { get; set; }
        public User Admin { get; set; } = null!;

        public string Action { get; set; } = string.Empty;   // e.g. "BanUser", "ResolveReport"

        public string? TargetType { get; set; }               // e.g. "User", "Report", "Build"
        public Guid? TargetId { get; set; }
        public string? TargetName { get; set; }               // human-readable label

        public string? Detail { get; set; }                   // extra context (ban reason, etc.)

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
