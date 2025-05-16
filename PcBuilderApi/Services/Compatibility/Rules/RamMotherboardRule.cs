using Microsoft.IdentityModel.Tokens;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility.Rules
{
    public class RamMotherboardRule : ICompatibilityRule
    {
        public string Name => "RAM and Motherboard Compatibility Rule";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            CompatibilityResult result = new CompatibilityResult();
            var pcBuild_Rams = pcBuild.PcBuild_Rams;
            var motherboard = pcBuild.Motherboard;
            if (pcBuild_Rams.IsNullOrEmpty() || motherboard == null)
            {
                return result;
            }
            if (motherboard.DimmType == null || motherboard.DimmFrequency == null)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Неможливо перевірити сумісність Оперативної пам'яті та Материнської плати — недостатньо даних."
                });
                return result;
            }
            int moduleCount = 0;
            int overallSize = 0;
            List<int> frequencies = new List<int>();
            foreach (var pcBuild_Ram in pcBuild_Rams)
            {
                var ram = pcBuild_Ram.Ram;
                if (ram == null)
                {
                    continue;
                }
                if (ram.Type != null && ram.Type != motherboard.DimmType)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Problem,
                        Message = $"Материнська плата не підтримує тип Оперативної пам'яті {ram.Type}."
                    });
                    continue;
                }
                if (motherboard.DimmFrequency != null && ram.Frequency > motherboard.DimmFrequency)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Problem,
                        Message = $"Частота Оперативної пам'яті ({ram.Name}) вища за максимально дозволену для Материнської плати ({motherboard.DimmFrequency})."
                    });
                    continue;
                }
                if (!frequencies.Contains(ram.Frequency))
                {
                    frequencies.Add(ram.Frequency);
                }
                if (ram.ModuleQuantity != null)
                {
                    moduleCount += (int)ram.ModuleQuantity * pcBuild_Ram.Quantity;
                    if (ram.Capacity != null)
                    {
                        overallSize += (int)ram.Capacity * (int)ram.ModuleQuantity * pcBuild_Ram.Quantity;
                    }
                }
            }

            if (result.Messages.Any(m => m.Type == CompatibilityMessageType.Problem) == true)
            {
                return result;
            }

            if (motherboard.DimmSlots != null && motherboard.DimmSlots < moduleCount)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Материнська плата має недостатню кількість слотів для Оперативної пам'яті. Кількість слотів: {motherboard.DimmSlots}, кількість модулів: {moduleCount}."
                });
            }
            if (motherboard.DimmCapacity != null && overallSize > motherboard.DimmCapacity)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Загальний об'єм Оперативної пам'яті ({overallSize}) вищий за максимально дозволений для Материнської плати ({motherboard.DimmCapacity})."
                });
            }
            if (frequencies.Distinct().Count() > 1)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Планки оперативної пам'яті працюють на різних максимальних частотах. Оперативна пам'ять працюватиме на нижчій швидкості ({frequencies.Min()}) замість ({frequencies.Max()})."
                });
            }
            return result;
        }
    }
}
