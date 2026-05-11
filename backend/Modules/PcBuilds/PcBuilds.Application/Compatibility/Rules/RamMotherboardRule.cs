using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class RamMotherboardRule : ICompatibilityRule
    {
        public string Name => "RamMotherboard";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var pcBuild_Rams = pcBuild.PcBuild_Rams;
            var motherboard = pcBuild.Motherboard;

            if (pcBuild_Rams == null || !pcBuild_Rams.Any() || motherboard == null)
                return result;

            if (motherboard.DimmType == null || motherboard.DimmFrequency == null)
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.RamTypeMismatch,
                    Parameters = new() { ["MbName"] = motherboard.Name ?? "" }
                });
                return result;
            }

            int moduleCount = 0;
            int overallSize = 0;
            var frequencies = new List<int>();

            foreach (var pcBuild_Ram in pcBuild_Rams)
            {
                var ram = pcBuild_Ram.Ram;
                if (ram == null) continue;

                if (ram.Type != null && ram.Type != motherboard.DimmType)
                {
                    result.FitnessScore = 0.0;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Critical,
                        Code = IssueCodes.RamTypeMismatch,
                        Parameters = new()
                        {
                            ["RamName"] = ram.Name ?? "",
                            ["RamType"] = ram.Type,
                            ["MbDimmType"] = motherboard.DimmType
                        }
                    });
                    continue;
                }

                if (motherboard.DimmFrequency != null && ram.Frequency > motherboard.DimmFrequency)
                {
                    result.FitnessScore = 0.0;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Critical,
                        Code = IssueCodes.RamFrequencyExceeded,
                        Parameters = new()
                        {
                            ["RamName"] = ram.Name ?? "",
                            ["RamFrequency"] = ram.Frequency.ToString(),
                            ["MbMaxFrequency"] = motherboard.DimmFrequency.ToString()!
                        }
                    });
                    continue;
                }

                if (!frequencies.Contains(ram.Frequency))
                    frequencies.Add(ram.Frequency);

                if (ram.ModuleQuantity != null)
                {
                    moduleCount += (int)ram.ModuleQuantity * pcBuild_Ram.Quantity;
                    if (ram.Capacity != null)
                        overallSize += (int)ram.Capacity * (int)ram.ModuleQuantity * pcBuild_Ram.Quantity;
                }
            }

            if (result.Issues.Any(i => i.Severity == CompatibilitySeverity.Critical))
                return result;

            if (motherboard.DimmSlots != null && motherboard.DimmSlots < moduleCount)
            {
                result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.RamSlotsExceeded,
                    Parameters = new()
                    {
                        ["MbName"] = motherboard.Name ?? "",
                        ["MbSlots"] = motherboard.DimmSlots.ToString()!,
                        ["ModuleCount"] = moduleCount.ToString()
                    }
                });
            }

            if (motherboard.DimmCapacity != null && overallSize > motherboard.DimmCapacity)
            {
                result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.RamCapacityExceeded,
                    Parameters = new()
                    {
                        ["MbName"] = motherboard.Name ?? "",
                        ["MaxCapacity"] = motherboard.DimmCapacity.ToString()!,
                        ["TotalCapacity"] = overallSize.ToString()
                    }
                });
            }

            if (frequencies.Distinct().Count() > 1)
            {
                result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.RamMixedFrequencies,
                    Parameters = new()
                    {
                        ["MinFrequency"] = frequencies.Min().ToString(),
                        ["MaxFrequency"] = frequencies.Max().ToString()
                    }
                });
            }

            return result;
        }
    }
}
