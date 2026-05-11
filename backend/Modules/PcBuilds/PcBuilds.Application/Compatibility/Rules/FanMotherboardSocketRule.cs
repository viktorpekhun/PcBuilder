using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class FanMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "FanMotherboardHeaders";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var pcBuild_Fans = pcBuild.PcBuild_Fans;
            var motherboard = pcBuild.Motherboard;

            if (pcBuild_Fans == null || !pcBuild_Fans.Any() || motherboard == null)
                return result;

            if (motherboard.FanQuantity == null)
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.FanHeadersInsufficient,
                    Parameters = new() { ["MbName"] = motherboard.Name ?? "" }
                });
                return result;
            }

            int fanQuantity = 0;
            foreach (var pcBuild_Fan in pcBuild_Fans)
            {
                var fan = pcBuild_Fan.Fan;
                if (fan == null) continue;

                if (fan.ModuleCount == null)
                {
                    result.FitnessScore = 0.7;
                    result.Issues.Add(new CompatibilityIssue
                    {
                        Severity = CompatibilitySeverity.Warning,
                        Code = IssueCodes.FanHeadersInsufficient,
                        Parameters = new() { ["FanName"] = fan.Name ?? "", ["MbName"] = motherboard.Name ?? "" }
                    });
                    continue;
                }
                fanQuantity += pcBuild_Fan.Quantity * (int)fan.ModuleCount;
            }

            if (motherboard.FanQuantity < fanQuantity)
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.FanHeadersInsufficient,
                    Parameters = new()
                    {
                        ["MbName"] = motherboard.Name ?? "",
                        ["MbHeaders"] = motherboard.FanQuantity.ToString()!,
                        ["FanCount"] = fanQuantity.ToString()
                    }
                });
            }

            return result;
        }
    }
}
