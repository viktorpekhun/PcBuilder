using Components.Domain.Entities;

namespace PcBuilds.Application.Compatibility.PreFilter
{
    public record PartialBuild(
        Cpu? Cpu = null,
        Gpu? Gpu = null,
        Motherboard? Motherboard = null,
        Ram? Ram = null,
        CpuCooler? CpuCooler = null,
        PcCase? PcCase = null,
        PowerSupply? PowerSupply = null,
        Ssd? Ssd = null,
        Hdd? Hdd = null,
        Fan? Fan = null,
        int FanQuantity = 0);
}
