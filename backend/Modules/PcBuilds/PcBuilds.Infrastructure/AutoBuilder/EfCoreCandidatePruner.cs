using Components.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.AutoBuilder.Models;

namespace PcBuilds.Infrastructure.AutoBuilder
{
    public class EfCoreCandidatePruner : ICandidatePruner
    {
        private const int TopN = 20;
        private const int TopNSmall = 8;
        // Must cover the assembler's slot caps (up to 3.0× allocation for cooler).
        // A lower value silently excludes components the assembler is allowed to pick.
        private const double BudgetHeadroom = 2.5;

        private readonly IApplicationDbContext _context;

        public EfCoreCandidatePruner(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CandidatePool> PruneAsync(AutoBuildRequestDto request, ScenarioPolicy policy, CancellationToken ct)
        {
            var a = policy.Allocation;
            var min = policy.Minimums;
            var budget = request.Budget;

            // EF Core DbContext is not thread-safe — queries must run sequentially
            var cpus = await QueryCpus(budget, a.Cpu, request, ct);
            var gpus = await QueryGpus(budget, a.Gpu, request, ct);
            var mbs = await QueryMotherboards(budget, a.Motherboard, request, ct);
            var coolers = await QueryCoolers(budget, a.Cooler, request, ct);
            var psus = await QueryPsus(budget, a.Psu, min.MinPsuW, ct);
            var cases = await QueryCases(budget, a.Case, request, ct);
            var rams = await QueryRams(budget, a.Ram, request, min.MinRamGb, ct);
            var ssds = await QuerySsds(budget, a.Ssd, request, min.MinSsdGb, ct);
            var hdds = await QueryHdds(budget, a.Hdd, min.MinHddGb, ct);
            var fans = await QueryFans(budget, a.Fans, ct);

            return new CandidatePool(cpus, gpus, mbs, coolers, psus, cases, rams, ssds, hdds, fans);
        }

        private async Task<IReadOnlyList<Cpu>> QueryCpus(decimal budget, double alloc, AutoBuildRequestDto req, CancellationToken ct)
        {
            var cap = budget * (decimal)alloc;
            var q = _context.Set<Cpu>()
                .Where(c => c.AveragePrice.HasValue && c.AveragePrice > 0 && c.AveragePrice <= cap
                            && c.PassMarkScore > 0);

            if (req.IsFutureProof)
                q = q.Where(c => !c.Socket.Contains("1151") && !c.Socket.Contains("1200") && !c.Socket.Contains("AM4"));

            return await q.OrderByDescending(c => c.AveragePrice * c.OffersCount).Take(TopN * 3).ToListAsync(ct);
        }

        private async Task<IReadOnlyList<Gpu>> QueryGpus(decimal budget, double alloc, AutoBuildRequestDto req, CancellationToken ct)
        {
            var cap = budget * (decimal)alloc;
            var q = _context.Set<Gpu>()
                .Include(g => g.GpuPowerConnectors)
                .Where(g => g.AveragePrice.HasValue && g.AveragePrice > 0 && g.AveragePrice <= cap
                            && g.PassMarkScore > 0);

            if (req.IsFutureProof)
                q = q.Where(g => g.PcleVersion >= 4.0);

            return await q.OrderByDescending(g => g.AveragePrice * g.OffersCount).Take(TopN * 3).ToListAsync(ct);
        }

        private async Task<IReadOnlyList<Motherboard>> QueryMotherboards(decimal budget, double alloc, AutoBuildRequestDto req, CancellationToken ct)
        {
            var cap = budget * (decimal)alloc * (decimal)BudgetHeadroom;
            var q = _context.Set<Motherboard>()
                .Include(m => m.CpuPowerConnectors)
                .Include(m => m.PcleSlots)
                .Include(m => m.M2Slots)
                .Where(m => m.AveragePrice.HasValue && m.AveragePrice > 0 && m.AveragePrice <= cap);

            if (req.IsFutureProof)
                q = q.Where(m => m.DimmType != "DDR4" && m.DimmType != "DDR3");

            if (req.PreferredFormFactor != null)
                q = q.Where(m => m.FormFactor == req.PreferredFormFactor);

            // Wide pool: assembler needs a socket-matching board for every CPU candidate.
            return await q.OrderBy(m => m.AveragePrice).Take(TopN * 4).ToListAsync(ct);
        }

        private async Task<IReadOnlyList<CpuCooler>> QueryCoolers(decimal budget, double alloc, AutoBuildRequestDto req, CancellationToken ct)
        {
            var cap = budget * (decimal)alloc * (decimal)BudgetHeadroom;
            // Wide pool: CoolerSelector picks by power-dissipation/price ratio, so it benefits
            // from variety across the full price range. Sort price-DESC so high-end coolers
            // (better dissipation) are guaranteed in the pool for high-TDP CPUs.
            return await _context.Set<CpuCooler>()
                .Include(c => c.CpuCoolerSockets)
                .Where(c => c.AveragePrice.HasValue && c.AveragePrice > 0 && c.AveragePrice <= cap)
                .OrderByDescending(c => c.AveragePrice)
                .Take(TopN * 5)
                .ToListAsync(ct);
        }

        private async Task<IReadOnlyList<PowerSupply>> QueryPsus(decimal budget, double alloc, int minPsuW, CancellationToken ct)
        {
            var cap = budget * (decimal)alloc * (decimal)BudgetHeadroom;
            // Wide pool: actual required wattage depends on the chosen CPU+GPU and can range
            // from 450W to 1200W+. The assembler picks cheapest sufficient, so we need PSUs
            // across the wattage spectrum.
            return await _context.Set<PowerSupply>()
                .Include(p => p.PowerSupplyPowerConnectors)
                .Where(p => p.AveragePrice.HasValue && p.AveragePrice > 0 && p.AveragePrice <= cap
                            && p.Wattage >= minPsuW)
                .OrderByDescending(p => p.Wattage)
                .ThenBy(p => p.AveragePrice)
                .Take(TopN * 5)
                .ToListAsync(ct);
        }

        private async Task<IReadOnlyList<PcCase>> QueryCases(decimal budget, double alloc, AutoBuildRequestDto req, CancellationToken ct)
        {
            var cap = budget * (decimal)alloc * (decimal)BudgetHeadroom;
            var q = _context.Set<PcCase>()
                .Include(c => c.PcCaseFormFactors)
                .Include(c => c.PcCaseFanLocations)
                .Where(c => c.AveragePrice.HasValue && c.AveragePrice > 0 && c.AveragePrice <= cap);

            if (req.PreferredFormFactor != null)
                q = q.Where(c => c.PcCaseFormFactors.Any(ff => ff.Name == req.PreferredFormFactor));

            // Cases: price-DESC so the pool covers the upper end (assembler picks most expensive within cap).
            return await q.OrderByDescending(c => c.AveragePrice).Take(TopN * 2).ToListAsync(ct);
        }

        private async Task<IReadOnlyList<Ram>> QueryRams(decimal budget, double alloc, AutoBuildRequestDto req, int minRamGb, CancellationToken ct)
        {
            var cap = budget * (decimal)alloc * (decimal)BudgetHeadroom;
            // Restrict to current-gen DDR4/DDR5 — older types (DDR2/DDR3) win the GB/UAH ratio
            // with tiny vintage modules that no modern motherboard accepts
            var q = _context.Set<Ram>()
                .Where(r => r.AveragePrice.HasValue && r.AveragePrice > 0 && r.AveragePrice <= cap
                            && r.Capacity.HasValue && r.ModuleQuantity.HasValue
                            && r.Capacity.Value * r.ModuleQuantity.Value >= minRamGb
                            && (r.Type == "DDR4" || r.Type == "DDR5"));

            if (req.IsFutureProof)
                q = q.Where(r => r.Type != "DDR4");

            // Sort by total capacity DESC (matches assembler's "max capacity within cap" selection).
            // Ratio sorting (capacity/price) always wins with cheap small kits and starves
            // high-budget builds of large kits.
            var ddr4 = await q.Where(r => r.Type == "DDR4")
                .OrderByDescending(r => r.Capacity!.Value * r.ModuleQuantity!.Value)
                .ThenBy(r => r.AveragePrice)
                .Take(TopNSmall * 2).ToListAsync(ct);
            var ddr5 = await q.Where(r => r.Type == "DDR5")
                .OrderByDescending(r => r.Capacity!.Value * r.ModuleQuantity!.Value)
                .ThenBy(r => r.AveragePrice)
                .Take(TopNSmall * 2).ToListAsync(ct);

            return ddr4.Concat(ddr5).ToList();
        }

        private async Task<IReadOnlyList<Ssd>> QuerySsds(decimal budget, double alloc, AutoBuildRequestDto req, int minSsdGb, CancellationToken ct)
        {
            var cap = budget * (decimal)alloc * (decimal)BudgetHeadroom;
            var q = _context.Set<Ssd>()
                .Where(s => s.AveragePrice.HasValue && s.AveragePrice > 0 && s.AveragePrice <= cap
                            && s.Capacity >= minSsdGb);

            if (req.IsFutureProof)
                q = q.Where(s => s.Interface != null && s.Interface.Contains("NVMe"));

            // Capacity-DESC: matches assembler's "max capacity within cap" selection.
            return await q
                .OrderByDescending(s => s.Capacity)
                .ThenBy(s => s.AveragePrice)
                .Take(TopNSmall * 2)
                .ToListAsync(ct);
        }

        private async Task<IReadOnlyList<Hdd>> QueryHdds(decimal budget, double alloc, int? minHddGb, CancellationToken ct)
        {
            if (alloc <= 0 || !minHddGb.HasValue) return Array.Empty<Hdd>();

            var cap = budget * (decimal)alloc * (decimal)BudgetHeadroom;
            var minHdd = minHddGb.Value;
            // Capacity-DESC: matches assembler's "max capacity within cap" selection.
            return await _context.Set<Hdd>()
                .Where(h => h.AveragePrice.HasValue && h.AveragePrice > 0 && h.AveragePrice <= cap
                            && h.Capacity >= minHdd)
                .OrderByDescending(h => h.Capacity)
                .ThenBy(h => h.AveragePrice)
                .Take(TopNSmall * 2)
                .ToListAsync(ct);
        }

        private async Task<IReadOnlyList<Fan>> QueryFans(decimal budget, double alloc, CancellationToken ct)
        {
            if (alloc <= 0) return Array.Empty<Fan>();

            var cap = budget * (decimal)alloc * (decimal)BudgetHeadroom;
            return await _context.Set<Fan>()
                .Where(f => f.AveragePrice.HasValue && f.AveragePrice > 0 && f.AveragePrice <= cap)
                .OrderBy(f => f.AveragePrice)
                .Take(TopNSmall * 2)
                .ToListAsync(ct);
        }

    }
}
