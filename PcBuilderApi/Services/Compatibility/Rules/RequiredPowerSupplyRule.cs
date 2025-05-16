using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility.Rules
{
    public class RequiredPowerSupplyRule : ICompatibilityRule
    {
        public string Name => "Required Power Supply Rule";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var powerSupply = pcBuild.PowerSupply;
            if (powerSupply == null)
            {
                return result;
            }
            var cpu = pcBuild.Cpu;
            var gpu = pcBuild.Gpu;
            var motherboard = pcBuild.Motherboard;
            var cpuCooler = pcBuild.CpuCooler;
            var pcBuild_Rams = pcBuild.PcBuild_Rams;
            var pcBuild_Ssds = pcBuild.PcBuild_Ssds;
            var pcBuild_Hdds = pcBuild.PcBuild_Hdds;
            var pcBuild_Fans = pcBuild.PcBuild_Fans;

            int summaryPower = 0;
            if (cpu != null)
            {
                summaryPower += (int)cpu.Tdp;
            }
            if (gpu != null)
            {
                summaryPower += (int)gpu.Wattage;
            }
            if (motherboard != null)
            {
                summaryPower += (int)motherboard.Wattage;
            }
            if (cpuCooler != null)
            {
                summaryPower += (int)cpuCooler.Wattage;
            }
            if (pcBuild_Rams != null)
            {
                foreach (var pcBuild_Ram in pcBuild_Rams)
                {
                    var ram = pcBuild_Ram.Ram;
                    if (ram != null)
                    {
                        summaryPower += (int)ram.Wattage*(int)ram.ModuleQuantity*pcBuild_Ram.Quantity;
                    }
                }
            }
            if (pcBuild_Ssds != null)
            {
                foreach (var pcBuild_Ssd in pcBuild_Ssds)
                {
                    var ssd = pcBuild_Ssd.Ssd;
                    if (ssd != null)
                    {
                        summaryPower += (int)ssd.Wattage * pcBuild_Ssd.Quantity;
                    }
                }
            }
            if (pcBuild_Hdds != null)
            {
                foreach (var pcBuild_Hdd in pcBuild_Hdds)
                {
                    var hdd = pcBuild_Hdd.Hdd;
                    if (hdd != null)
                    {
                        summaryPower += (int)hdd.Wattage * pcBuild_Hdd.Quantity;
                    }
                }
            }
            if (pcBuild_Fans != null)
            {
                foreach (var pcBuild_Fan in pcBuild_Fans)
                {
                    var fan = pcBuild_Fan.Fan;
                    if (fan != null)
                    {
                        summaryPower += (int)fan.Wattage * (int)fan.ModuleCount * pcBuild_Fan.Quantity;
                    }
                }
            }
            summaryPower += summaryPower * 3 / 10;

            if (summaryPower > powerSupply.Wattage)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Problem,
                    Message = $"Потужність Блоку живлення ({powerSupply.Wattage} Вт) недостатня для живлення всіх компонентів системи ({summaryPower} Вт)."
                });
            }

            return result;
        }
    }
}
