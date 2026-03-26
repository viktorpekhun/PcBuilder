
using Components.Domain.Entities;
using PcBuilds.Domain.Entities;
using System.Text.RegularExpressions;
using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class SsdMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "SSD and Motherboard Socket Compatibility";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var pcBuild_Ssds = pcBuild.PcBuild_Ssds;
            var motherboard = pcBuild.Motherboard;


            if (pcBuild_Ssds == null || !pcBuild_Ssds.Any() || motherboard == null)
            {
                return result;
            }
            int ssdM2Quantity = 0;
            int ssdSataQuantity = 0;
            foreach (var pcBuild_Ssd in pcBuild_Ssds)
            {
                var ssd = pcBuild_Ssd.Ssd;
                int quantity = pcBuild_Ssd.Quantity;
                if (ssd == null)
                {
                    continue;
                }
                if (ssd.Interface == null || ((motherboard.M2Slots==null || !motherboard.M2Slots.Any()) && ssd.Interface.ToLower().Contains("m.2")))
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = $"Неможливо перевірити сумісність SSD ({ssd.Name}) та Материнської плати — недостатньо даних."
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
                            result.Messages.Add(new CompatibilityMessage
                            {
                                Type = CompatibilityMessageType.Warning,
                                Message = $"Слоти M.2 Материнської плати мають старішу версію, ніж у M.2 у SSD ({ssd.Name}). SSD може працювати на зниженій швидкості."
                            });
                        }
                    }
                    else
                    {
                        result.Messages.Add(new CompatibilityMessage
                        {
                            Type = CompatibilityMessageType.Warning,
                            Message = $"Не вдалося перевірити версію роз'єму М.2 SSD ({ssd.Name}) та Материнської плати. SSD може працювати на зниженій швидкості."
                        });
                    }
                    ssdM2Quantity += quantity;
                }
                else if (ssd.Interface.ToLower().Contains("sata"))
                {
                    if (motherboard.Sata3Count == null)
                    {
                        result.Messages.Add(new CompatibilityMessage
                        {
                            Type = CompatibilityMessageType.Warning,
                            Message = $"Не вдалося перевірити сумісність SSD ({ssd.Name}) та Материнської плати - дедостантньо даних."
                        });
                    }
                    ssdSataQuantity += quantity;
                }
            }

            if (motherboard.M2Slots.Count != 0)
            {
                int? mbM2Slots = 0;
                foreach (var m2Slot in motherboard.M2Slots)
                {
                    mbM2Slots += m2Slot.Quantity;
                }
                if (ssdM2Quantity > mbM2Slots)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = $"Кількість SSD M.2 ({ssdM2Quantity}) перевищує кількість слотів M.2 на Материнській платі."
                    });
                }
            }
            var pcBuild_Hdds = pcBuild.PcBuild_Hdds;
            int takenSata = 0;
            if (pcBuild_Hdds.Count() != 0)
            {
                foreach (var pcBuild_Hdd in pcBuild_Hdds)
                {
                    takenSata += pcBuild_Hdd.Quantity;
                }
            }
            if ((motherboard.Sata3Count-takenSata) < ssdSataQuantity && ssdSataQuantity > 0)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Кількість SSD SATA ({ssdSataQuantity}) перевищує кількість вільних слотів SATA на материнській платі."
                });
            }

            return result;
        }
    }

}
