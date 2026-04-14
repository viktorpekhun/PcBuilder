namespace PcBuilder.Api.Models
{
    public class ScrapeJobStatus
    {
        public Guid JobId { get; init; }
        public string ComponentType { get; init; } = string.Empty;
        public string State { get; set; } = "Queued";
        public DateTime QueuedAt { get; init; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int ItemsScraped { get; set; }
        public int? TotalItems { get; set; }
    }
}
