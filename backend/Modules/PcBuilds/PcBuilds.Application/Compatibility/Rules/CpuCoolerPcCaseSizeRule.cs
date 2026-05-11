using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class CpuCoolerPcCaseSizeRule : ICompatibilityRule
    {
        public string Name => "CoolerCaseFit";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var cpuCooler = pcBuild.CpuCooler;
            var pcCase = pcBuild.PcCase;

            if (cpuCooler == null || pcCase == null)
                return result;

            if (cpuCooler.Type == "Air")
            {
                if (cpuCooler.Height == null || pcCase.MaxCpuCoolerHeight == null)
                {
                    result.FitnessScore = 0.7;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Warning,
                        Code = IssueCodes.CoolerCaseDataMissing,
                        Parameters = new() { ["CoolerName"] = cpuCooler.Name ?? "", ["CaseName"] = pcCase.Name ?? "" }
                    });
                    return result;
                }
                if (cpuCooler.Height > pcCase.MaxCpuCoolerHeight)
                {
                    result.FitnessScore = 0.0;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Critical,
                        Code = IssueCodes.CoolerCaseFit,
                        Parameters = new()
                        {
                            ["CoolerName"] = cpuCooler.Name ?? "",
                            ["CoolerHeight"] = cpuCooler.Height.ToString()!,
                            ["CaseName"] = pcCase.Name ?? "",
                            ["MaxHeight"] = pcCase.MaxCpuCoolerHeight.ToString()!
                        }
                    });
                }
            }
            else
            {
                var pcCaseFanLocations = pcCase.PcCaseFanLocations;
                if (cpuCooler.FanCount == null || cpuCooler.FanSize == null || pcCaseFanLocations == null || !pcCaseFanLocations.Any())
                {
                    result.FitnessScore = 0.7;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Warning,
                        Code = IssueCodes.CoolerCaseDataMissing,
                        Parameters = new() { ["CoolerName"] = cpuCooler.Name ?? "", ["CaseName"] = pcCase.Name ?? "" }
                    });
                    return result;
                }
                var matches = pcCaseFanLocations.Where(x => x.FanSize == cpuCooler.FanSize && x.MaxFans >= cpuCooler.FanCount).ToList();
                if (matches.Count == 0)
                {
                    result.FitnessScore = 0.0;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Critical,
                        Code = IssueCodes.CoolerCaseFit,
                        Parameters = new()
                        {
                            ["CoolerName"] = cpuCooler.Name ?? "",
                            ["CaseName"] = pcCase.Name ?? "",
                            ["FanSize"] = cpuCooler.FanSize.ToString()!,
                            ["FanCount"] = cpuCooler.FanCount.ToString()!
                        }
                    });
                }
            }

            return result;
        }
    }
}
