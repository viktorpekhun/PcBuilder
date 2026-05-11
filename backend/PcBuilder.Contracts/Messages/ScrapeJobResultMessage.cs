namespace PcBuilder.Contracts.Messages
{
    public record ScrapeJobResultMessage(
        Guid JobId,
        string ComponentType,
        bool Success,
        string? ErrorMessage,
        DateTime CompletedAt,
        int ItemsScraped
    );
}
