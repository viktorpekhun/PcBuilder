namespace PcBuilder.Contracts.Messages
{
    public record ScrapeJobMessage(
        Guid JobId,
        string Url,
        string ComponentType,
        string EntityTypeName,
        string Kind,
        string[]? NestedTypeNames,
        bool CorrectGpuModels
    );
}
