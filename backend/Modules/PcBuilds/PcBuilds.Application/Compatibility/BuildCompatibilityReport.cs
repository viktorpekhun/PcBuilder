using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility
{
    public class BuildCompatibilityReport
    {
        public bool IsStrictlyCompatible { get; set; }

        /// <summary>
        /// Geometric mean of all per-rule FitnessScores.
        /// Forced to 0.0 if any rule has a Critical issue.
        /// </summary>
        public double OverallFitnessScore { get; set; }

        public List<CompatibilityResult> RuleResults { get; set; } = new();

        public static BuildCompatibilityReport From(List<CompatibilityResult> results)
        {
            bool hasCritical = results.Any(r => !r.IsStrictlyCompatible);
            double overallScore = hasCritical ? 0.0 : ComputeGeometricMean(results);

            return new BuildCompatibilityReport
            {
                IsStrictlyCompatible = !hasCritical,
                OverallFitnessScore = overallScore,
                RuleResults = results
            };
        }

        private static double ComputeGeometricMean(List<CompatibilityResult> results)
        {
            if (results.Count == 0) return 1.0;
            double product = results.Aggregate(1.0, (acc, r) => acc * r.FitnessScore);
            return Math.Pow(product, 1.0 / results.Count);
        }
    }
}
