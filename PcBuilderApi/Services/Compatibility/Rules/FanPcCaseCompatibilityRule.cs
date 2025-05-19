using Microsoft.IdentityModel.Tokens;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility.Rules
{
    public class FanPcCaseCompatibilityRule : ICompatibilityRule
    {
        public string Name => "Fan and PC Case Compatibility Rule";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            CompatibilityResult result = new CompatibilityResult();
            var pcBuild_Fans = pcBuild.PcBuild_Fans;
            var pcCase = pcBuild.PcCase;
            if (pcBuild_Fans == null || !pcBuild_Fans.Any() || pcCase == null)
            {
                return result;
            }
            var pcCaseFanSlots = pcCase.PcCaseFanLocations;

            if (pcCaseFanSlots == null || !pcCaseFanSlots.Any())
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Неможливо перевірити сумісність вентилятора/рів та Корпусу ПК — недостатньо даних."
                });
                return result;
            }

            Dictionary<int, int> fanCount = new Dictionary<int, int>();
            foreach (var pcBuild_Fan in pcBuild_Fans)
            {
                var fan = pcBuild_Fan.Fan;
                if (fan == null)
                {
                    continue;
                }
                if (fan.ModuleCount == null || fan.SizeLength == null)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = $"Неможливо перевірити сумісність вентилятора/рів ({fan.Name}) та Корпусу ПК — недостатньо даних."
                    });
                    continue;
                }
                int fanSize = (int)fan.SizeLength.Value;
                if (fanCount.ContainsKey(fanSize))
                {
                    fanCount[fanSize] += pcBuild_Fan.Quantity * (int)fan.ModuleCount;
                }
                else
                {
                    fanCount.Add(fanSize, pcBuild_Fan.Quantity * (int)fan.ModuleCount);
                }
            }

            Dictionary<int, int> pcCaseFanSlotsCount = new Dictionary<int, int>();
            foreach (var pcCaseFanSlot in pcCaseFanSlots)
            {
                if (pcCaseFanSlotsCount.ContainsKey(pcCaseFanSlot.FanSize))
                {
                    pcCaseFanSlotsCount[pcCaseFanSlot.FanSize] += pcCaseFanSlot.MaxFans;
                }
                else
                {
                    pcCaseFanSlotsCount.Add(pcCaseFanSlot.FanSize, pcCaseFanSlot.MaxFans);
                }
            }
            foreach (var fanSize in fanCount.Keys)
            {
                if (pcCaseFanSlotsCount.ContainsKey(fanSize))
                {
                    if (fanCount[fanSize] > pcCaseFanSlotsCount[fanSize])
                    {
                        result.Messages.Add(new CompatibilityMessage
                        {
                            Type = CompatibilityMessageType.Warning,
                            Message = $"Кількість вентиляторів ({fanCount[fanSize]}) розміру {fanSize} мм перевищує максимальну кількість, яку підтримує корпус ПК ({pcCaseFanSlotsCount[fanSize]})."
                        });
                    }
                }
                else
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Problem,
                        Message = $"Корпус ПК не підтримує вентилятори розміру {fanSize}."
                    });
                }
            }

            return result;
        }
    }
}
