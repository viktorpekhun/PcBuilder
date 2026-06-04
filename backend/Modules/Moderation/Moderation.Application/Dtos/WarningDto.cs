using Moderation.Domain.Enums;

namespace Moderation.Application.Dtos
{
    public class WarningDto
    {
        public Guid Id { get; set; }
        public BanType BanType { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? ReasonCode { get; set; }
        public string? IssuedByAdminUsername { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
