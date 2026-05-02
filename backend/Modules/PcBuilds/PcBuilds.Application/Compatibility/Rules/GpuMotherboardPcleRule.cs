using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class GpuMotherboardPcleRule : ICompatibilityRule
    {
        public string Name => "GpuPcie";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var gpu = pcBuild.Gpu;
            var motherboard = pcBuild.Motherboard;

            if (gpu == null || motherboard == null)
                return result;

            var pcleSlots = motherboard.PcleSlots;

            if (gpu.PcleVersion == null || pcleSlots == null || !pcleSlots.Any())
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.GpuPcieVersion,
                    Parameters = new() { ["GpuName"] = gpu.Name ?? "", ["MbName"] = motherboard.Name ?? "" }
                });
                return result;
            }

            bool hasPartialCompatibility = false;
            bool hasVersionMismatch = false;

            foreach (var pcleSlot in pcleSlots)
            {
                if (pcleSlot.Version >= gpu.PcleVersion)
                {
                    if (pcleSlot.Lane >= gpu.PcleLane)
                        return result;

                    hasPartialCompatibility = true;
                }
                else
                {
                    hasVersionMismatch = true;
                }
            }

            if (hasPartialCompatibility)
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.GpuPcieLane,
                    Parameters = new() { ["GpuName"] = gpu.Name ?? "", ["MbName"] = motherboard.Name ?? "" }
                });
            }
            else if (hasVersionMismatch)
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.GpuPcieVersion,
                    Parameters = new() { ["GpuName"] = gpu.Name ?? "", ["MbName"] = motherboard.Name ?? "" }
                });
            }

            return result;
        }
    }
}
