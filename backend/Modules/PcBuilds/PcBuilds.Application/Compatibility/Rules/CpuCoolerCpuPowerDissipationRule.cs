using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class CpuCoolerCpuPowerDissipationRule : ICompatibilityRule
    {
        public string Name => "CoolerTdp";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var cpuCooler = pcBuild.CpuCooler;
            var cpu = pcBuild.Cpu;

            if (cpu == null || cpuCooler == null)
                return result;

            if (cpuCooler.MaxPowerDissipation == null || cpu.Tdp == null)
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.CoolerTdpDataMissing,
                    Parameters = new() { ["CoolerName"] = cpuCooler.Name ?? "", ["CpuName"] = cpu.Name ?? "" }
                });
                return result;
            }

            if (cpuCooler.MaxPowerDissipation < cpu.Tdp)
            {
                result.FitnessScore = 0.0;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Critical,
                    Code = IssueCodes.CoolerTdpInsufficient,
                    Parameters = new()
                    {
                        ["CoolerName"] = cpuCooler.Name ?? "",
                        ["CoolerTdp"] = cpuCooler.MaxPowerDissipation.ToString()!,
                        ["CpuName"] = cpu.Name ?? "",
                        ["CpuTdp"] = cpu.Tdp.ToString()!
                    }
                });
            }

            return result;
        }
    }
}
