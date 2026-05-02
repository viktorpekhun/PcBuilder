using PcBuilder.SharedKernel.Enums;
using PcBuilds.Application.Compatibility.Internal;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class CpuGpuBottleneckRule : ICompatibilityRule
    {
        private const double K = 1.2;
        private const double LowerBound = 0.6;
        private const double UpperBound = 1.5;
        private const double BalancedLow = 0.85;
        private const double BalancedHigh = 1.25;

        public string Name => "CpuGpuBottleneck";

        public CompatibilityResult Check(PcBuild build)
        {
            var result = new CompatibilityResult();

            if (build.Cpu == null || build.Gpu == null)
                return result;

            if (build.Cpu.PassMarkScore == 0 || build.Gpu.PassMarkScore == 0)
                return result;

            var ratio = build.Gpu.PassMarkScore / (build.Cpu.PassMarkScore * K);

            result.FitnessScore = ComputeFitnessScore(ratio);

            if (ratio < LowerBound)
            {
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.GpuBottleneck,
                    Parameters = new()
                    {
                        ["CpuName"] = build.Cpu.Name ?? string.Empty,
                        ["GpuName"] = build.Gpu.Name ?? string.Empty,
                        ["Ratio"] = ratio.ToString("F2")
                    }
                });
            }
            else if (ratio > UpperBound)
            {
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.CpuBottleneck,
                    Parameters = new()
                    {
                        ["CpuName"] = build.Cpu.Name ?? string.Empty,
                        ["GpuName"] = build.Gpu.Name ?? string.Empty,
                        ["Ratio"] = ratio.ToString("F2")
                    }
                });
            }

            return result;
        }

        private static double ComputeFitnessScore(double ratio)
        {
            if (ratio >= BalancedLow && ratio <= BalancedHigh)
                return 1.0;

            if (ratio >= LowerBound && ratio < BalancedLow)
                return FitnessMath.Lerp(0.5, 1.0, (ratio - LowerBound) / (BalancedLow - LowerBound));

            if (ratio > BalancedHigh && ratio <= UpperBound)
                return FitnessMath.Lerp(1.0, 0.5, (ratio - BalancedHigh) / (UpperBound - BalancedHigh));

            if (ratio < LowerBound)
                return Math.Max(0.1, FitnessMath.Lerp(0.1, 0.5, ratio / LowerBound));

            // ratio > UpperBound
            return Math.Max(0.1, FitnessMath.Lerp(0.5, 0.1, (ratio - UpperBound) / UpperBound));
        }
    }
}
