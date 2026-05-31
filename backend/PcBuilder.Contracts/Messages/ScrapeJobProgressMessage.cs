namespace PcBuilder.Contracts.Messages
{
    public record ScrapeJobProgressMessage(
        Guid JobId,
        string ComponentType,
        int ItemsScraped,
        int? TotalItems
    );
}
