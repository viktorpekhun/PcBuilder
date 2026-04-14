using Moderation.Domain.Enums;

namespace Moderation.Application.Dtos
{
    public class ReportDto
    {
        public Guid Id { get; set; }
        public Guid ReporterId { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public Guid ReportedEntityId { get; set; }
        public Guid ReportedUserId { get; set; }
        public string ReportedUsername { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public string? AdminResolutionNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
