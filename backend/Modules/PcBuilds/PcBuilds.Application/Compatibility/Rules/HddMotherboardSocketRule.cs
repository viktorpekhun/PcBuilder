using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class HddMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "HddSataSlots";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var pcBuild_Hdds = pcBuild.PcBuild_Hdds;
            var motherboard = pcBuild.Motherboard;

            if (pcBuild_Hdds == null || !pcBuild_Hdds.Any() || motherboard == null)
                return result;

            if (motherboard.Sata3Count == 0)
            {
                result.FitnessScore = 0.0;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Critical,
                    Code = IssueCodes.SataSlotsMissing,
                    Parameters = new() { ["MbName"] = motherboard.Name ?? "" }
                });
                return result;
            }

            int hddSata3Quantity = 0;
            foreach (var pcBuild_Hdd in pcBuild_Hdds)
            {
                var hdd = pcBuild_Hdd.Hdd;
                if (hdd == null) continue;

                hddSata3Quantity += pcBuild_Hdd.Quantity;
                if (motherboard.Sata3Count == null)
                {
                    result.FitnessScore = 0.7;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Warning,
                        Code = IssueCodes.SataSlotsMissing,
                        Parameters = new() { ["HddName"] = hdd.Name ?? "", ["MbName"] = motherboard.Name ?? "" }
                    });
                }
            }

            int ssdSata3Quantity = 0;
            foreach (var pcBuild_Ssd in pcBuild.PcBuild_Ssds)
            {
                var ssd = pcBuild_Ssd.Ssd;
                if (ssd == null) continue;

                if (ssd.Interface != null && ssd.Interface.Contains("sata", StringComparison.OrdinalIgnoreCase))
                    ssdSata3Quantity += pcBuild_Ssd.Quantity;
            }

            if (motherboard.Sata3Count != null && motherboard.Sata3Count < hddSata3Quantity + ssdSata3Quantity)
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.SataSlotsExceeded,
                    Parameters = new()
                    {
                        ["MbName"] = motherboard.Name ?? "",
                        ["MbSataSlots"] = motherboard.Sata3Count.ToString()!,
                        ["HddCount"] = hddSata3Quantity.ToString(),
                        ["SsdCount"] = ssdSata3Quantity.ToString()
                    }
                });
            }

            return result;
        }
    }
}
