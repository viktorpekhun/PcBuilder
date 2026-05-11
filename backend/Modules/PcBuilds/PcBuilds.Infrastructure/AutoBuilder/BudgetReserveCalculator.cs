using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.AutoBuilder.Models;
using PcBuilds.Application.Compatibility.PreFilter;

namespace PcBuilds.Infrastructure.AutoBuilder
{
    public class BudgetReserveCalculator
    {
        private readonly IComponentCompatibilityFilter _filter;

        public BudgetReserveCalculator(IComponentCompatibilityFilter filter) => _filter = filter;

        /// <summary>
        /// Returns the cheapest price among the slot's pool that passes compatibility
        /// against the current partial build. Returns 0 when no candidates exist (caller
        /// treats as "no reserve needed" — the slot will simply fail if the pool is empty).
        /// </summary>
        public decimal MinViablePrice(SlotKind kind, PartialBuild ctx, CandidatePool pool, ComfortThresholds min)
        {
            var prices = kind switch
            {
                SlotKind.Motherboard => _filter.FilterMotherboards(pool.Motherboards.AsQueryable(), ctx)
                                               .Select(m => m.AveragePrice ?? 0m),

                SlotKind.Ram         => _filter.FilterRams(pool.Rams.AsQueryable(), ctx)
                                               .Where(r => r.Capacity * r.ModuleQuantity >= min.MinRamGb)
                                               .Select(r => r.AveragePrice ?? 0m),

                SlotKind.Cooler      => _filter.FilterCoolers(pool.CpuCoolers.AsQueryable(), ctx)
                                               .Select(c => c.AveragePrice ?? 0m),

                SlotKind.Ssd         => _filter.FilterSsds(pool.Ssds.AsQueryable(), ctx)
                                               .Where(s => s.Capacity >= min.MinSsdGb)
                                               .Select(s => s.AveragePrice ?? 0m),

                SlotKind.Hdd         => _filter.FilterHdds(pool.Hdds.AsQueryable(), ctx)
                                               .Where(h => h.Capacity >= (min.MinHddGb ?? 0))
                                               .Select(h => h.AveragePrice ?? 0m),

                SlotKind.Case        => _filter.FilterCases(pool.PcCases.AsQueryable(), ctx)
                                               .Select(c => c.AveragePrice ?? 0m),

                SlotKind.Fan         => _filter.FilterFans(pool.Fans.AsQueryable(), ctx)
                                               .Select(f => f.AveragePrice ?? 0m),

                SlotKind.Psu         => pool.PowerSupplies
                                            .Where(p => p.Wattage >= min.MinPsuW)
                                            .Select(p => p.AveragePrice ?? 0m)
                                            .AsQueryable(),

                _ => Enumerable.Empty<decimal>().AsQueryable()
            };

            return prices.DefaultIfEmpty(0m).Min();
        }
    }
}
