namespace PcBuilds.Application.AutoBuilder
{
    public record AutoBuildRequestDto(
        decimal Budget,
        UsageScenario PreferredUse,
        bool IsStrictBudget,
        bool IsFutureProof = false,
        string? PreferredFormFactor = null);
}
