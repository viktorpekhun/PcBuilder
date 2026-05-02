using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility
{
    public class CompatibilityResult
    {
        public string RuleName { get; set; } = string.Empty;

        /// <summary>Continuous score 0.0–1.0. Defaults to 1.0; rules set it in Phase 3.</summary>
        public double FitnessScore { get; set; } = 1.0;

        public List<CompatibilityIssue> Issues { get; set; } = new();

        public bool IsStrictlyCompatible =>
            Issues.All(i => i.Severity != CompatibilitySeverity.Critical);
    }
}
