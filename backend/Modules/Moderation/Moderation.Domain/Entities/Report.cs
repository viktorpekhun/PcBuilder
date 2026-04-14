using System.ComponentModel.DataAnnotations;
using Auth.Domain.Entities;
using Moderation.Domain.Enums;

namespace Moderation.Domain.Entities
{
    public class Report
    {
        public Guid Id { get; set; }

        public Guid ReporterId { get; set; }
        public User Reporter { get; set; } = null!;

        public ReportType ReportType { get; set; }

        public Guid ReportedEntityId { get; set; }

        public Guid ReportedUserId { get; set; }
        public User ReportedUser { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "Reason can't have more than 500 characters.")]
        public string Reason { get; set; } = string.Empty;

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        [MaxLength(500)]
        public string? AdminResolutionNote { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByAdminId { get; set; }
        public User? ResolvedByAdmin { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
