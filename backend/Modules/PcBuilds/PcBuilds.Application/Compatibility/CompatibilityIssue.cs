using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility
{
    public class CompatibilityIssue
    {
        public CompatibilitySeverity Severity { get; init; }

        /// <summary>
        /// Standardized code, e.g. "CPU_BOTTLENECK", "PSU_INSUFFICIENT".
        /// Frontend uses this as an i18n key. "LEGACY_PLACEHOLDER" is a bridge
        /// value used during Phase 2 migration; Phase 3 replaces it with proper codes.
        /// </summary>
        public string Code { get; init; } = string.Empty;

        /// <summary>
        /// Frontend injects these into the translated string, e.g. {CpuName, Ratio}.
        /// During Phase 2, "text" holds the original Ukrainian string for fallback.
        /// </summary>
        public Dictionary<string, string> Parameters { get; init; } = new();
    }
}
