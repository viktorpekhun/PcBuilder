using Components.Domain.Entities;

namespace PcBuilds.Application.AutoBuilder
{
    public interface ICoolerSelector
    {
        CpuCooler? PickCooler(Cpu cpu, IReadOnlyList<CpuCooler> coolerCandidates);
        int InferMaxPowerDissipation(CpuCooler cooler);
    }
}
