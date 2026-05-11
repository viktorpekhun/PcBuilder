using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;
using System.Text.RegularExpressions;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class SsdMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "SsdSlots";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var pcBuild_Ssds = pcBuild.PcBuild_Ssds;
            var motherboard = pcBuild.Motherboard;

            if (pcBuild_Ssds == null || !pcBuild_Ssds.Any() || motherboard == null)
                return result;

            int ssdM2Quantity = 0;
            int ssdSataQuantity = 0;

            foreach (var pcBuild_Ssd in pcBuild_Ssds)
            {
                var ssd = pcBuild_Ssd.Ssd;
                int quantity = pcBuild_Ssd.Quantity;
                if (ssd == null) continue;

                if (ssd.Interface == null || ((motherboard.M2Slots == null || !motherboard.M2Slots.Any()) && ssd.Interface.ToLower().Contains("m.2")))
                {
                    result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Warning,
                        Code = IssueCodes.M2SlotsMissing,
                        Parameters = new() { ["SsdName"] = ssd.Name ?? "", ["MbName"] = motherboard.Name ?? "" }
                    });
                    continue;
                }

                if (ssd.Interface.ToLower().Contains("m.2"))
                {
                    var match = Regex.Match(ssd.Interface, @"\((?:[^)]*?)(\d+)");
                    if (match.Success)
                    {
                        var version = double.Parse(match.Groups[1].Value);
                        var m2Slots = motherboard.M2Slots;
                        var matches = m2Slots.Where(x => x.Version >= version).ToList();
                        if (matches.Count == 0)
                        {
                            result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                            result.Issues.Add(new CompatibilityIssue
                            {
                                Severity = CompatibilitySeverity.Warning,
                                Code = IssueCodes.NvmeVersionMismatch,
                                Parameters = new()
                                {
                                    ["SsdName"] = ssd.Name ?? "",
                                    ["MbName"] = motherboard.Name ?? "",
                                    ["RequiredVersion"] = version.ToString()
                                }
                            });
                        }
                    }
                    else
                    {
                        result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                        result.Issues.Add(new CompatibilityIssue
                        {
                            Severity = CompatibilitySeverity.Warning,
                            Code = IssueCodes.NvmeVersionMismatch,
                            Parameters = new() { ["SsdName"] = ssd.Name ?? "", ["MbName"] = motherboard.Name ?? "" }
                        });
                    }
                    ssdM2Quantity += quantity;
                }
                else if (ssd.Interface.ToLower().Contains("sata"))
                {
                    if (motherboard.Sata3Count == null)
                    {
                        result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                        result.Issues.Add(new CompatibilityIssue
                        {
                            Severity = CompatibilitySeverity.Warning,
                            Code = IssueCodes.SataSlotsMissing,
                            Parameters = new() { ["SsdName"] = ssd.Name ?? "", ["MbName"] = motherboard.Name ?? "" }
                        });
                    }
                    ssdSataQuantity += quantity;
                }
            }

            if (motherboard.M2Slots.Count != 0)
            {
                int? mbM2Slots = 0;
                foreach (var m2Slot in motherboard.M2Slots)
                    mbM2Slots += m2Slot.Quantity;

                if (ssdM2Quantity > mbM2Slots)
                {
                    result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Warning,
                        Code = IssueCodes.M2SlotsExceeded,
                        Parameters = new()
                        {
                            ["MbName"] = motherboard.Name ?? "",
                            ["M2SlotCount"] = mbM2Slots.ToString()!,
                            ["SsdCount"] = ssdM2Quantity.ToString()
                        }
                    });
                }
            }

            int takenSata = 0;
            if (pcBuild.PcBuild_Hdds.Any())
            {
                foreach (var pcBuild_Hdd in pcBuild.PcBuild_Hdds)
                    takenSata += pcBuild_Hdd.Quantity;
            }

            if ((motherboard.Sata3Count - takenSata) < ssdSataQuantity && ssdSataQuantity > 0)
            {
                result.FitnessScore = Math.Min(result.FitnessScore, 0.7);
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.SataSlotsExceeded,
                    Parameters = new()
                    {
                        ["MbName"] = motherboard.Name ?? "",
                        ["FreeSataSlots"] = (motherboard.Sata3Count - takenSata).ToString()!,
                        ["SsdCount"] = ssdSataQuantity.ToString()
                    }
                });
            }

            return result;
        }
    }
}
