using Components.Domain.Entities;

namespace Components.Application.PreFilter
{
    /// <summary>
    /// SQL-translatable pre-filter for each component slot, driven by a partial build context
    /// composed from already-resolved entity objects.
    ///
    /// This interface is consumed by <c>GetComponentsByTypeHandler</c>. The full interface
    /// (including in-memory PSU rules) lives in <c>PcBuilds.Application.Compatibility.PreFilter.IComponentCompatibilityFilter</c>.
    /// </summary>
    public interface IComponentQueryPreFilter
    {
        IQueryable<Cpu>         FilterCpus         (IQueryable<Cpu> q,         PartialBuildContext ctx);
        IQueryable<Gpu>         FilterGpus         (IQueryable<Gpu> q,         PartialBuildContext ctx);
        IQueryable<Motherboard> FilterMotherboards (IQueryable<Motherboard> q, PartialBuildContext ctx);
        IQueryable<Ram>         FilterRams         (IQueryable<Ram> q,         PartialBuildContext ctx);
        IQueryable<CpuCooler>   FilterCoolers      (IQueryable<CpuCooler> q,   PartialBuildContext ctx);
        IQueryable<PcCase>      FilterCases        (IQueryable<PcCase> q,      PartialBuildContext ctx);
        IQueryable<PowerSupply> FilterPowerSupplies(IQueryable<PowerSupply> q, PartialBuildContext ctx);
        IQueryable<Fan>         FilterFans         (IQueryable<Fan> q,         PartialBuildContext ctx);
    }
}
