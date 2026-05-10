using Components.Domain.Entities;

namespace PcBuilds.Application.AutoBuilder.Models
{
    public record CandidatePool(
        IReadOnlyList<Cpu> Cpus,
        IReadOnlyList<Gpu> Gpus,
        IReadOnlyList<Motherboard> Motherboards,
        IReadOnlyList<CpuCooler> CpuCoolers,
        IReadOnlyList<PowerSupply> PowerSupplies,
        IReadOnlyList<PcCase> PcCases,
        IReadOnlyList<Ram> Rams,
        IReadOnlyList<Ssd> Ssds,
        IReadOnlyList<Hdd> Hdds,
        IReadOnlyList<Fan> Fans);
}
