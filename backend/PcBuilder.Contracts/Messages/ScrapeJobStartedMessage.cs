namespace PcBuilder.Contracts.Messages
{
    public record ScrapeJobStartedMessage(
        Guid JobId,
        string ComponentType,
        DateTime StartedAt
    );
}
