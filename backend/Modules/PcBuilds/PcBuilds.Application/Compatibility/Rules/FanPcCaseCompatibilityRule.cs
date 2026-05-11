using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class FanPcCaseCompatibilityRule : ICompatibilityRule
    {
        public string Name => "FanCaseSlots";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var pcBuild_Fans = pcBuild.PcBuild_Fans;
            var pcCase = pcBuild.PcCase;

            if (pcBuild_Fans == null || !pcBuild_Fans.Any() || pcCase == null)
                return result;

            var pcCaseFanSlots = pcCase.PcCaseFanLocations;
            if (pcCaseFanSlots == null || !pcCaseFanSlots.Any())
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.FanCaseDataMissing,
                    Parameters = new() { ["CaseName"] = pcCase.Name ?? "" }
                });
                return result;
            }

            var fanCount = new Dictionary<int, int>();
            foreach (var pcBuild_Fan in pcBuild_Fans)
            {
                var fan = pcBuild_Fan.Fan;
                if (fan == null) continue;

                if (fan.ModuleCount == null || fan.SizeLength == null)
                {
                    result.FitnessScore = 0.7;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Warning,
                        Code = IssueCodes.FanCaseDataMissing,
                        Parameters = new() { ["FanName"] = fan.Name ?? "", ["CaseName"] = pcCase.Name ?? "" }
                    });
                    continue;
                }

                int fanSize = (int)fan.SizeLength.Value;
                if (fanCount.ContainsKey(fanSize))
                    fanCount[fanSize] += pcBuild_Fan.Quantity * (int)fan.ModuleCount;
                else
                    fanCount.Add(fanSize, pcBuild_Fan.Quantity * (int)fan.ModuleCount);
            }

            var pcCaseFanSlotsCount = new Dictionary<int, int>();
            foreach (var pcCaseFanSlot in pcCaseFanSlots)
            {
                if (pcCaseFanSlotsCount.ContainsKey(pcCaseFanSlot.FanSize))
                    pcCaseFanSlotsCount[pcCaseFanSlot.FanSize] += pcCaseFanSlot.MaxFans;
                else
                    pcCaseFanSlotsCount.Add(pcCaseFanSlot.FanSize, pcCaseFanSlot.MaxFans);
            }

            foreach (var fanSize in fanCount.Keys)
            {
                if (pcCaseFanSlotsCount.ContainsKey(fanSize))
                {
                    if (fanCount[fanSize] > pcCaseFanSlotsCount[fanSize])
                    {
                        result.FitnessScore = 0.7;
                        result.Issues.Add(new CompatibilityIssue
                        {
                            Severity = CompatibilitySeverity.Warning,
                            Code = IssueCodes.FanCaseSlotsInsufficient,
                            Parameters = new()
                            {
                                ["CaseName"] = pcCase.Name ?? "",
                                ["FanSize"] = fanSize.ToString(),
                                ["FanCount"] = fanCount[fanSize].ToString(),
                                ["MaxSlots"] = pcCaseFanSlotsCount[fanSize].ToString()
                            }
                        });
                    }
                }
                else
                {
                    result.FitnessScore = 0.0;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Critical,
                        Code = IssueCodes.FanCaseSlotsInsufficient,
                        Parameters = new()
                        {
                            ["CaseName"] = pcCase.Name ?? "",
                            ["FanSize"] = fanSize.ToString(),
                            ["FanCount"] = fanCount[fanSize].ToString(),
                            ["MaxSlots"] = "0"
                        }
                    });
                }
            }

            return result;
        }
    }
}
