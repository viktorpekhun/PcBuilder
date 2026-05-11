using Components.Domain.Entities;

namespace PcBuilds.Application.Compatibility.PreFilter
{
    /// <summary>
    /// Translates a <see cref="PartialBuild"/> into IQueryable Where predicates for each
    /// component slot. Methods are pure (no side effects, no DB context) and composable —
    /// each method only knows about its direct dependencies.
    ///
    /// No-data policy: if the source component in <paramref name="ctx"/> lacks the required
    /// field (e.g. CPU.Socket is null/empty), the method returns q.Where(_ => false) rather
    /// than silently passing the full pool through. An unfiltered pool is never "all compatible."
    /// </summary>
    public interface IComponentCompatibilityFilter
    {
        // ── SQL-translatable predicates ───────────────────────────────────────────
        // Safe to chain before pagination — EF Core translates all predicates to SQL.

        /// <summary>Filters CPUs by motherboard socket.</summary>
        IQueryable<Cpu> FilterCpus(IQueryable<Cpu> q, PartialBuild ctx);

        /// <summary>Filters GPUs by case max GPU length.</summary>
        IQueryable<Gpu> FilterGpus(IQueryable<Gpu> q, PartialBuild ctx);

        /// <summary>
        /// Filters motherboards by CPU socket, RAM DDR type/frequency, and case form factor.
        /// </summary>
        IQueryable<Motherboard> FilterMotherboards(IQueryable<Motherboard> q, PartialBuild ctx);

        /// <summary>Filters RAM by motherboard DDR type and max frequency.</summary>
        IQueryable<Ram> FilterRams(IQueryable<Ram> q, PartialBuild ctx);

        /// <summary>
        /// Filters CPU coolers by motherboard socket, CPU TDP, and case max cooler height (Air coolers).
        /// </summary>
        IQueryable<CpuCooler> FilterCoolers(IQueryable<CpuCooler> q, PartialBuild ctx);

        /// <summary>
        /// Filters cases by motherboard form factor, GPU length, and cooler height.
        /// </summary>
        IQueryable<PcCase> FilterCases(IQueryable<PcCase> q, PartialBuild ctx);

        /// <summary>
        /// Filters power supplies using SQL-translatable predicates only (wattage is skipped here;
        /// use <see cref="ApplyInMemoryPsuRules"/> for full connector + wattage checks).
        /// </summary>
        IQueryable<PowerSupply> FilterPowerSupplies(IQueryable<PowerSupply> q, PartialBuild ctx);

        /// <summary>Filters fans by case fan slot sizes.</summary>
        IQueryable<Fan> FilterFans(IQueryable<Fan> q, PartialBuild ctx);

        /// <summary>
        /// Filters SSDs by motherboard slot availability:
        /// M.2 SSDs require the motherboard to have ≥1 M.2 slot;
        /// SATA SSDs require Sata3Count &gt; 0. Unknown/missing interface data passes through.
        /// </summary>
        IQueryable<Ssd> FilterSsds(IQueryable<Ssd> q, PartialBuild ctx);

        /// <summary>
        /// Filters HDDs by motherboard SATA availability (Sata3Count &gt; 0). HDDs are always SATA.
        /// </summary>
        IQueryable<Hdd> FilterHdds(IQueryable<Hdd> q, PartialBuild ctx);

        // ── In-memory rules ───────────────────────────────────────────────────────
        // PSU connector pin-matching and wattage estimation cannot be expressed as SQL.
        // Used by the autobuilder after .ToList(); NOT used by the paged UI endpoint
        // (which uses only the SQL-translatable FilterPowerSupplies above).

        /// <summary>
        /// Applies PSU connector compatibility (CPU pins, GPU pins) and wattage checks
        /// in memory. Call after materialising the SQL-filtered PSU list.
        /// </summary>
        IEnumerable<PowerSupply> ApplyInMemoryPsuRules(IEnumerable<PowerSupply> psus, PartialBuild ctx);
    }
}
