
using Components.Domain.Entities;
using PcBuilds.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class FanMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "Fan and Motherboard Socket Compatibility Rule";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            CompatibilityResult result = new CompatibilityResult();
            var pcBuild_Fans = pcBuild.PcBuild_Fans;
            var motherboard = pcBuild.Motherboard;
            if (pcBuild_Fans==null || !pcBuild_Fans.Any() || motherboard == null)
            {
                return result;
            }
            if (motherboard.FanQuantity == null)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Неможливо перевірити сумісність вентилятора/рів та Материнської плати — недостатньо даних."
                });
                return result;
            }
            int fanQuantity = 0;
            foreach (var pcBuild_Fan in pcBuild_Fans)
            {
                var fan = pcBuild_Fan.Fan;
                if (fan == null)
                {
                    continue;
                }
                if (fan.ModuleCount == null)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = $"Неможливо перевірити сумісність вентилятора/рів ({fan.Name}) та Материнської плати — недостатньо даних."
                    });
                    continue;
                }
                fanQuantity += pcBuild_Fan.Quantity * (int)fan.ModuleCount;
            }
            if (motherboard.FanQuantity < fanQuantity)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Материнська плата має недостатню кількість сокетів для підключення вентиляторів. Кількість роз'ємів: {motherboard.FanQuantity}, кількість вентиляторів: {fanQuantity}. Придбайте розгалужувач або приберіть зайві вентилятори."
                });
            }
            return result;
        }
    }
}
