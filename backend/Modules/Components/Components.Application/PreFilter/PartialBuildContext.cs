using Components.Domain.Entities;

namespace Components.Application.PreFilter
{
    /// <summary>
    /// Entity-resolved version of <see cref="PartialBuildIds"/>.
    /// Built by <c>GetComponentsByTypeHandler</c> after loading the referenced entities from DB.
    /// </summary>
    public record PartialBuildContext(
        Cpu?          Cpu          = null,
        Gpu?          Gpu          = null,
        Motherboard?  Motherboard  = null,
        Ram?          Ram          = null,
        CpuCooler?    CpuCooler    = null,
        PcCase?       PcCase       = null,
        PowerSupply?  PowerSupply  = null);
}
