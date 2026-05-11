namespace PcBuilder.Persistence.Data.Entities
{
    public class ScrapeJob
    {
        public Guid Id { get; set; }
        public string ComponentType { get; set; } = string.Empty;
        public string Kind { get; set; } = "Category";
        public string State { get; set; } = "Queued";
        public DateTime QueuedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int ItemsScraped { get; set; }
        public int? TotalItems { get; set; }
    }
}
