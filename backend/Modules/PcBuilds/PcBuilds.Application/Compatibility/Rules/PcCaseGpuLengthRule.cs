using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class PcCaseGpuLengthRule : ICompatibilityRule
    {
        public string Name => "GpuCaseLength";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var gpu = pcBuild.Gpu;
            var pcCase = pcBuild.PcCase;

            if (gpu == null || pcCase == null)
                return result;

            if (gpu.SizeLength == null || pcCase.MaxGpuLength == null)
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.GpuLengthDataMissing,
                    Parameters = new() { ["GpuName"] = gpu.Name ?? "", ["CaseName"] = pcCase.Name ?? "" }
                });
                return result;
            }

            if (gpu.SizeLength > pcCase.MaxGpuLength)
            {
                result.FitnessScore = 0.0;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Critical,
                    Code = IssueCodes.GpuLengthExceedsCase,
                    Parameters = new()
                    {
                        ["GpuName"] = gpu.Name ?? "",
                        ["GpuLength"] = gpu.SizeLength.ToString()!,
                        ["CaseName"] = pcCase.Name ?? "",
                        ["MaxLength"] = pcCase.MaxGpuLength.ToString()!
                    }
                });
            }

            return result;
        }
    }
}
