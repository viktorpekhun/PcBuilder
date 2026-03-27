
using Components.Domain.Entities;
using PcBuilds.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class HddMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "HDD and Motherboard Socket Compatibility Rule";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            CompatibilityResult result = new CompatibilityResult();
            var pcBuild_Hdds = pcBuild.PcBuild_Hdds;
            var motherboard = pcBuild.Motherboard;

            if (pcBuild_Hdds == null || !pcBuild_Hdds.Any() || motherboard == null)
            {
                return result;
            }
            if (motherboard.Sata3Count == 0)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Problem,
                    Message = $"Материнська плата не має SATA сокетів для HDD."
                });
                return result;
            }
            int hddSata3Quantity = 0;
            foreach (var pcBuild_Hdd in pcBuild_Hdds)
            {
                var hdd = pcBuild_Hdd.Hdd;
                if (hdd == null)
                {
                    continue;
                }
                hddSata3Quantity += pcBuild_Hdd.Quantity;
                if (motherboard.Sata3Count == null)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = $"Неможливо перевірити сумісність HDD ({hdd.Name}) та Материнської плати — недостатньо даних."
                    });
                    continue;
                }
            }

            var pcBuild_Ssds = pcBuild.PcBuild_Ssds;
            int ssdSata3Quantity = 0;
            foreach (var pcBuild_Ssd in pcBuild_Ssds)
            {
                var ssd = pcBuild_Ssd.Ssd;
                if (ssd == null)
                {
                    continue;
                }
                if (ssd.Interface != null && ssd.Interface.Contains("sata", StringComparison.OrdinalIgnoreCase))
                {
                    ssdSata3Quantity += pcBuild_Ssd.Quantity;
                }
            }
            if (motherboard.Sata3Count != null && motherboard.Sata3Count < hddSata3Quantity + ssdSata3Quantity)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Кількість HDD ({hddSata3Quantity}) перевищує кількість вільних слотів SATA на материнській платі."
                });
            }
            return result;

        }
    }

}
