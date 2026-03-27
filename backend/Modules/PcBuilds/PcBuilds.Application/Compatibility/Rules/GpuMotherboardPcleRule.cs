
using Components.Domain.Entities;
using PcBuilds.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class GpuMotherboardPcleRule : ICompatibilityRule
    {
        public string Name => "GPU and Motherboard PCIe Compatibility";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var gpu = pcBuild.Gpu;
            var motherboard = pcBuild.Motherboard;
            if (gpu == null || motherboard == null)
            {
                return result;
            }

            var pcleSlots = motherboard.PcleSlots;

            if (gpu.PcleVersion == null || pcleSlots == null || !pcleSlots.Any())
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = "Неможливо перевірити сумісність Відеокарти та Материнської плати — недостатньо даних."
                });
                return result;
            }

            bool hasPartialCompatibility = false;
            bool hasVersionMismatch = false;
            foreach (var pcleSlot in pcleSlots)
            {
                if (pcleSlot.Version >= gpu.PcleVersion)
                {
                    if (pcleSlot.Lane >= gpu.PcleLane)
                    {
                        return result;
                    }

                    hasPartialCompatibility = true;
                }
                else
                {
                    hasVersionMismatch = true;
                }
            }

            if (hasPartialCompatibility)
            {
                if (gpu.PcleLane != null)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = $"Недостатньо даних про пропускну здібність PCIe Відеокарти. Відеокарта може працювати на зниженій швидкості."
                    });
                }
                else
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = $"Слоти PCIe x16 Материнської плати мають меншу пропускну здібність, ніж у PCIe у Відеокарті. Відеокарта може працювати на зниженій швидкості."
                    });
                }
            }
            else if (hasVersionMismatch)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Слоти PCIe x16 Материнської плати мають старішу версію, ніж у PCIe у Відеокарті. Відеокарта може працювати на зниженій швидкості."
                });
            }

            return result;
        }
    }
}
