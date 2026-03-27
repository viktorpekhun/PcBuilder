using Components.Domain.Entities;
using PcBuilds.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class CpuCoolerCpuPowerDissipationRule : ICompatibilityRule
    {
        public string Name => "CPU Cooler and CPU Power Dissipation Compatibility";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var cpuCooler = pcBuild.CpuCooler;
            var cpu = pcBuild.Cpu;

            if (cpu == null || cpuCooler == null)
            {
                return result;
            }

            if (cpuCooler?.MaxPowerDissipation == null || cpu?.Tdp == null)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = "Неможливо перевірити сумісність кулера CPU та CPU — недостатньо даних."
                });
                return result;
            }

            if (cpuCooler.MaxPowerDissipation < cpu.Tdp)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Problem,
                    Message = $"Максимальна потужність розсіювання кулера CPU ({cpuCooler.MaxPowerDissipation} Вт) менша за TDP CPU ({cpu.Tdp} Вт)."
                });
            }

            return result;
        }
    }
}
