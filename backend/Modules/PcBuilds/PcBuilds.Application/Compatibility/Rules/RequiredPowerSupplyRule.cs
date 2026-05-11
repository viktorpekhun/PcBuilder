using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Application.Compatibility.Internal;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class RequiredPowerSupplyRule : ICompatibilityRule
    {
        public string Name => "RequiredPowerSupply";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var powerSupply = pcBuild.PowerSupply;

            if (powerSupply == null)
                return result;

            int summaryPower = 0;

            if (pcBuild.Cpu != null)
                summaryPower += (int)pcBuild.Cpu.Tdp;

            if (pcBuild.Gpu != null)
            {
                if (pcBuild.Gpu.Wattage != null && pcBuild.Gpu.Wattage > 0)
                    summaryPower += (int)pcBuild.Gpu.Wattage;
                else if (pcBuild.Gpu.PsuReccomended != null && pcBuild.Gpu.PsuReccomended > 0)
                    summaryPower += (int)(pcBuild.Gpu.PsuReccomended * 0.45);
            }

            if (pcBuild.Motherboard != null)
                summaryPower += (int)pcBuild.Motherboard.Wattage;

            if (pcBuild.CpuCooler != null)
                summaryPower += (int)pcBuild.CpuCooler.Wattage;

            foreach (var pcBuild_Ram in pcBuild.PcBuild_Rams)
            {
                if (pcBuild_Ram.Ram != null)
                    summaryPower += (int)pcBuild_Ram.Ram.Wattage * (int)pcBuild_Ram.Ram.ModuleQuantity * pcBuild_Ram.Quantity;
            }

            foreach (var pcBuild_Ssd in pcBuild.PcBuild_Ssds)
            {
                if (pcBuild_Ssd.Ssd != null)
                    summaryPower += (int)pcBuild_Ssd.Ssd.Wattage * pcBuild_Ssd.Quantity;
            }

            foreach (var pcBuild_Hdd in pcBuild.PcBuild_Hdds)
            {
                if (pcBuild_Hdd.Hdd != null)
                    summaryPower += (int)pcBuild_Hdd.Hdd.Wattage * pcBuild_Hdd.Quantity;
            }

            foreach (var pcBuild_Fan in pcBuild.PcBuild_Fans)
            {
                if (pcBuild_Fan.Fan != null)
                    summaryPower += (int)pcBuild_Fan.Fan.Wattage * (int)pcBuild_Fan.Fan.ModuleCount * pcBuild_Fan.Quantity;
            }

            summaryPower += 150;

            var loadPct = (double)summaryPower / powerSupply.Wattage;

            result.FitnessScore = loadPct switch
            {
                > 1.0 => 0.0,
                >= 0.5 and <= 0.8 => 1.0,
                >= 0.4 and < 0.5 => FitnessMath.Lerp(0.85, 1.0, (loadPct - 0.4) / 0.1),
                >= 0.3 and < 0.4 => FitnessMath.Lerp(0.6, 0.85, (loadPct - 0.3) / 0.1),
                < 0.3 => 0.4,
                > 0.8 and <= 0.95 => FitnessMath.Lerp(1.0, 0.7, (loadPct - 0.8) / 0.15),
                > 0.95 => FitnessMath.Lerp(0.7, 0.0, (loadPct - 0.95) / 0.05),
                _ => 1.0
            };

            if (loadPct > 1.0)
            {
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Critical,
                    Code = IssueCodes.PsuInsufficient,
                    Parameters = new()
                    {
                        ["SystemWattage"] = summaryPower.ToString(),
                        ["PsuWattage"] = powerSupply.Wattage.ToString(),
                        ["LoadPercentage"] = (loadPct * 100).ToString("F0")
                    }
                });
            }
            else if (loadPct < 0.4)
            {
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Info,
                    Code = IssueCodes.PsuOverkill,
                    Parameters = new()
                    {
                        ["SystemWattage"] = summaryPower.ToString(),
                        ["PsuWattage"] = powerSupply.Wattage.ToString(),
                        ["LoadPercentage"] = (loadPct * 100).ToString("F0")
                    }
                });
            }

            return result;
        }
    }
}
