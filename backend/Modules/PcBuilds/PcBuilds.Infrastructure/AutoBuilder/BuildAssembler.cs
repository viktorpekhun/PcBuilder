using Components.Domain.Entities;
using Microsoft.Extensions.Logging;
using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.AutoBuilder.Models;
using PcBuilds.Application.Compatibility.PreFilter;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Infrastructure.AutoBuilder
{
    public class BuildAssembler : IBuildAssembler
    {
        private readonly ICoolerSelector _coolerSelector;
        private readonly IComponentCompatibilityFilter _filter;
        private readonly BudgetReserveCalculator _reserve;
        private readonly ILogger<BuildAssembler> _logger;

        private const int MaxBacktrackAttempts = 200;

        public BuildAssembler(
            ICoolerSelector coolerSelector,
            IComponentCompatibilityFilter filter,
            BudgetReserveCalculator reserve,
            ILogger<BuildAssembler> logger)
        {
            _coolerSelector = coolerSelector;
            _filter = filter;
            _reserve = reserve;
            _logger = logger;
        }

        public Task<PcBuild?> TryAssembleAsync(CorePairing core, AssemblyContext ctx, CancellationToken ct)
        {
            var budget = ctx.Request.Budget
                - core.Cpu.AveragePrice!.Value
                - core.Gpu.AveragePrice!.Value;

            if (budget < 0)
            {
                _logger.LogWarning("Assembler: budget exhausted before MB step (cpu+gpu cost > total budget)");
                return Task.FromResult<PcBuild?>(null);
            }

            var initialState = new AssemblyState(
                Partial: new PartialBuild(Cpu: core.Cpu, Gpu: core.Gpu),
                RemainingBudget: budget,
                Queue: BuildInitialSlotQueue(ctx.Policy));

            int attempts = 0;
            var result = TryFillSlot(0, initialState, core, ctx, ct, ref attempts);

            if (result is null)
                _logger.LogWarning(
                    "Assembler: no valid build found for CPU '{Cpu}' / GPU '{Gpu}' after {Attempts} attempt(s)",
                    core.Cpu.Name, core.Gpu.Name, attempts);

            return Task.FromResult(result);
        }

        // ── Recursive backtracking core ───────────────────────────────────────────

        private PcBuild? TryFillSlot(
            int slotIndex,
            AssemblyState state,
            CorePairing core,
            AssemblyContext ctx,
            CancellationToken ct,
            ref int attempts)
        {
            if (slotIndex >= state.Queue.Count)
                return BuildPcBuild(state, core, ctx);

            var slot = state.Queue[slotIndex];
            var pool = ctx.Pool;
            var min = ctx.Policy.Minimums;

            // Reserve = cheapest viable price for every slot still to come.
            decimal reserve = 0m;
            for (int j = slotIndex + 1; j < state.Queue.Count; j++)
                reserve += _reserve.MinViablePrice(state.Queue[j], state.Partial, pool, min);

            var spendCap = state.RemainingBudget - reserve;
            if (spendCap < 0)
            {
                _logger.LogDebug(
                    "Assembler: reserve {Reserve} exceeds remaining budget {Budget} at slot {Slot} — backtracking",
                    reserve, state.RemainingBudget, slot);
                return null;
            }

            foreach (var candidate in RankCandidates(slot, state, spendCap, core, ctx))
            {
                ct.ThrowIfCancellationRequested();

                if (++attempts > MaxBacktrackAttempts)
                {
                    _logger.LogWarning(
                        "Assembler: exceeded {Max} backtrack attempts — bailing",
                        MaxBacktrackAttempts);
                    return null;
                }

                var nextState = ApplyChoice(slot, candidate, state);
                var result = TryFillSlot(slotIndex + 1, nextState, core, ctx, ct, ref attempts);
                if (result is not null) return result;
            }

            _logger.LogDebug(
                "Assembler: all candidates exhausted at slot {Slot} — backtracking", slot);
            return null;
        }

        // ── Candidate ranking (per-slot filter + priority order) ─────────────────

        private IEnumerable<Candidate> RankCandidates(
            SlotKind slot,
            AssemblyState state,
            decimal spendCap,
            CorePairing core,
            AssemblyContext ctx)
        {
            var req = ctx.Request;
            var pool = ctx.Pool;
            var min = ctx.Policy.Minimums;
            var partial = state.Partial;
            var budget = state.RemainingBudget;

            switch (slot)
            {
                case SlotKind.Motherboard:
                {
                    var filtered = _filter.FilterMotherboards(pool.Motherboards.AsQueryable(), partial)
                        .Where(m => (!req.IsFutureProof || m.DimmType != "DDR4")
                                    && (req.PreferredFormFactor == null || m.FormFactor == req.PreferredFormFactor))
                        .ToList();

                    // Preferred: most expensive within spendCap; fallback: cheapest within budget
                    var preferred = filtered
                        .Where(m => m.AveragePrice <= spendCap)
                        .OrderByDescending(m => m.AveragePrice)
                        .Select(m => new Candidate(m.AveragePrice!.Value, Mb: m));

                    var fallback = filtered
                        .Where(m => m.AveragePrice > spendCap && m.AveragePrice <= budget)
                        .OrderBy(m => m.AveragePrice)
                        .Select(m => new Candidate(m.AveragePrice!.Value, Mb: m));

                    return preferred.Concat(fallback);
                }

                case SlotKind.Ram:
                {
                    var filtered = _filter.FilterRams(pool.Rams.AsQueryable(), partial)
                        .Where(r => r.Capacity.HasValue && r.ModuleQuantity.HasValue
                                    && r.Capacity.Value * r.ModuleQuantity.Value >= min.MinRamGb
                                    && (!req.IsFutureProof || r.Type != "DDR4"))
                        .ToList();

                    var preferred = filtered
                        .Where(r => r.AveragePrice <= spendCap)
                        .OrderByDescending(r => r.Capacity!.Value * r.ModuleQuantity!.Value)
                        .Select(r => new Candidate(r.AveragePrice!.Value, Ram: r));

                    var fallback = filtered
                        .Where(r => r.AveragePrice > spendCap && r.AveragePrice <= budget)
                        .OrderBy(r => r.AveragePrice)
                        .Select(r => new Candidate(r.AveragePrice!.Value, Ram: r));

                    return preferred.Concat(fallback);
                }

                case SlotKind.Cooler:
                {
                    var filtered = _filter.FilterCoolers(pool.CpuCoolers.AsQueryable(), partial)
                        .Where(c => c.AveragePrice <= spendCap)
                        .ToList();

                    var picked = _coolerSelector.PickCooler(core.Cpu, filtered);
                    if (picked is null) return [];
                    return [new Candidate(picked.AveragePrice!.Value, Cooler: picked)];
                }

                case SlotKind.Ssd:
                {
                    var filtered = _filter.FilterSsds(pool.Ssds.AsQueryable(), partial)
                        .Where(s => s.Capacity >= min.MinSsdGb
                                    && (!req.IsFutureProof || (s.Interface != null && s.Interface.Contains("NVMe"))))
                        .ToList();

                    var preferred = filtered
                        .Where(s => s.AveragePrice <= spendCap)
                        .OrderByDescending(s => s.Capacity)
                        .Select(s => new Candidate(s.AveragePrice!.Value, Ssd: s));

                    var fallback = filtered
                        .Where(s => s.AveragePrice > spendCap && s.AveragePrice <= budget)
                        .OrderBy(s => s.AveragePrice)
                        .Select(s => new Candidate(s.AveragePrice!.Value, Ssd: s));

                    return preferred.Concat(fallback);
                }

                case SlotKind.Hdd:
                {
                    var filtered = _filter.FilterHdds(pool.Hdds.AsQueryable(), partial)
                        .Where(h => h.Capacity >= min.MinHddGb!.Value)
                        .ToList();

                    var preferred = filtered
                        .Where(h => h.AveragePrice <= spendCap)
                        .OrderByDescending(h => h.Capacity)
                        .Select(h => new Candidate(h.AveragePrice!.Value, Hdd: h));

                    var fallback = filtered
                        .Where(h => h.AveragePrice > spendCap && h.AveragePrice <= budget)
                        .OrderBy(h => h.AveragePrice)
                        .Select(h => new Candidate(h.AveragePrice!.Value, Hdd: h));

                    return preferred.Concat(fallback);
                }

                case SlotKind.Case:
                {
                    var filtered = _filter.FilterCases(pool.PcCases.AsQueryable(), partial).ToList();

                    var preferred = filtered
                        .Where(c => c.AveragePrice <= spendCap)
                        .OrderByDescending(c => c.AveragePrice)
                        .Select(c => new Candidate(c.AveragePrice!.Value, PcCase: c));

                    var fallback = filtered
                        .Where(c => c.AveragePrice > spendCap && c.AveragePrice <= budget)
                        .OrderBy(c => c.AveragePrice)
                        .Select(c => new Candidate(c.AveragePrice!.Value, PcCase: c));

                    return preferred.Concat(fallback);
                }

                case SlotKind.Fan:
                {
                    // Fan is optional — always fall back to a skip so assembly never fails here.
                    var fanSkip = new Candidate(0m);

                    var pcCase = state.Partial.PcCase!;
                    var caseSlotsBySize = pcCase.PcCaseFanLocations
                        .GroupBy(fl => fl.FanSize)
                        .ToDictionary(g => g.Key, g => g.Sum(fl => fl.MaxFans));

                    var fanCandidates = _filter.FilterFans(pool.Fans.AsQueryable(), partial)
                        .Where(f => f.AveragePrice.HasValue && f.AveragePrice > 0)
                        .OrderBy(f => f.AveragePrice)
                        .ToList();

                    if (fanCandidates.Count == 0 || caseSlotsBySize.Count == 0)
                        return [fanSkip];

                    var matchingFan = fanCandidates.FirstOrDefault(f => f.AveragePrice <= budget);
                    if (matchingFan is null) return [fanSkip];

                    int sizeKey = (int)matchingFan.SizeLength!.Value;
                    if (!caseSlotsBySize.TryGetValue(sizeKey, out int slotsForSize))
                        return [fanSkip];

                    int moduleCount = matchingFan.ModuleCount ?? 1;
                    int maxPacks = moduleCount > 0 ? slotsForSize / moduleCount : slotsForSize;
                    int affordable = (int)(budget / matchingFan.AveragePrice!.Value);
                    int qty = Math.Max(1, Math.Min(maxPacks, affordable));
                    if (qty * matchingFan.AveragePrice.Value > budget)
                        return [fanSkip];

                    return [new Candidate(matchingFan.AveragePrice.Value * qty, Fan: matchingFan, FanQuantity: qty), fanSkip];
                }

                case SlotKind.Psu:
                {
                    var filtered = _filter.ApplyInMemoryPsuRules(pool.PowerSupplies, partial).ToList();

                    var preferred = filtered
                        .Where(p => p.AveragePrice <= spendCap)
                        .OrderBy(p => p.Wattage).ThenBy(p => p.AveragePrice)
                        .Select(p => new Candidate(p.AveragePrice!.Value, Psu: p));

                    var fallback = filtered
                        .Where(p => p.AveragePrice > spendCap && p.AveragePrice <= budget)
                        .OrderBy(p => p.Wattage).ThenBy(p => p.AveragePrice)
                        .Select(p => new Candidate(p.AveragePrice!.Value, Psu: p));

                    return preferred.Concat(fallback);
                }

                default:
                    return [];
            }
        }

        // ── State transition ──────────────────────────────────────────────────────

        private static AssemblyState ApplyChoice(SlotKind slot, Candidate c, AssemblyState state)
        {
            var next = state with { RemainingBudget = state.RemainingBudget - c.Price };

            return slot switch
            {
                SlotKind.Motherboard => next with
                {
                    Partial = next.Partial with { Motherboard = c.Mb },
                    Mb = c.Mb
                },
                SlotKind.Ram => next with
                {
                    Partial = next.Partial with { Ram = c.Ram },
                    Ram = c.Ram
                },
                SlotKind.Cooler => next with
                {
                    Partial = next.Partial with { CpuCooler = c.Cooler },
                    Cooler = c.Cooler
                },
                SlotKind.Ssd => next with
                {
                    Partial = next.Partial with { Ssd = c.Ssd },
                    Ssd = c.Ssd
                },
                SlotKind.Hdd => next with
                {
                    Partial = next.Partial with { Hdd = c.Hdd },
                    Hdd = c.Hdd
                },
                SlotKind.Case => next with
                {
                    Partial = next.Partial with { PcCase = c.PcCase },
                    PcCase = c.PcCase,
                    // Drop the Fan slot if the case ships with built-in fans
                    Queue = HasBuiltInFans(c.PcCase!)
                        ? next.Queue.Where(k => k != SlotKind.Fan).ToList()
                        : next.Queue
                },
                SlotKind.Fan => next with
                {
                    Partial = next.Partial with { Fan = c.Fan, FanQuantity = c.FanQuantity },
                    Fan = c.Fan,
                    FanQuantity = c.FanQuantity
                },
                SlotKind.Psu => next with { Psu = c.Psu },
                _ => next
            };
        }

        // ── Final build construction ──────────────────────────────────────────────

        private static PcBuild BuildPcBuild(AssemblyState state, CorePairing core, AssemblyContext ctx) =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = $"Auto Build ({ctx.Request.PreferredUse})",
                Cpu = core.Cpu,
                CpuId = core.Cpu.Id,
                Gpu = core.Gpu,
                GpuId = core.Gpu.Id,
                Motherboard = state.Mb,
                MotherboardId = state.Mb!.Id,
                CpuCooler = state.Cooler,
                CpuCoolerId = state.Cooler?.Id,
                PowerSupply = state.Psu,
                PowerSupplyId = state.Psu!.Id,
                PcCase = state.PcCase,
                PcCaseId = state.PcCase!.Id,
                PcBuild_Rams = new List<PcBuild_Ram>
                {
                    new() { Id = Guid.NewGuid(), Ram = state.Ram!, RamId = state.Ram!.Id, Quantity = 1 }
                },
                PcBuild_Ssds = new List<PcBuild_Ssd>
                {
                    new() { Id = Guid.NewGuid(), Ssd = state.Ssd!, SsdId = state.Ssd!.Id, Quantity = 1 }
                },
                PcBuild_Hdds = state.Hdd is not null
                    ? new List<PcBuild_Hdd> { new() { Id = Guid.NewGuid(), Hdd = state.Hdd, HddId = state.Hdd.Id, Quantity = 1 } }
                    : new List<PcBuild_Hdd>(),
                PcBuild_Fans = state.Fan is not null
                    ? new List<PcBuild_Fan> { new() { Id = Guid.NewGuid(), Fan = state.Fan, FanId = state.Fan.Id, Quantity = state.FanQuantity } }
                    : new List<PcBuild_Fan>(),
            };

        // ── Slot queue ────────────────────────────────────────────────────────────

        private static List<SlotKind> BuildInitialSlotQueue(ScenarioPolicy policy)
        {
            var q = new List<SlotKind>
            {
                SlotKind.Motherboard,
                SlotKind.Ram,
                SlotKind.Cooler,
                SlotKind.Ssd
            };
            if (policy.Minimums.MinHddGb.HasValue) q.Add(SlotKind.Hdd);
            q.Add(SlotKind.Case);
            q.Add(SlotKind.Fan);
            q.Add(SlotKind.Psu);
            return q;
        }

        private static bool HasBuiltInFans(PcCase pcCase) =>
            pcCase.BuiltInFans is { } bf
            && (!string.IsNullOrWhiteSpace(bf.Uk) || !string.IsNullOrWhiteSpace(bf.En));

        // ── Internal types ────────────────────────────────────────────────────────

        private sealed record AssemblyState(
            PartialBuild Partial,
            decimal RemainingBudget,
            IReadOnlyList<SlotKind> Queue,
            Motherboard? Mb = null,
            Ram? Ram = null,
            CpuCooler? Cooler = null,
            Ssd? Ssd = null,
            Hdd? Hdd = null,
            PcCase? PcCase = null,
            Fan? Fan = null,
            int FanQuantity = 0,
            PowerSupply? Psu = null);

        /// <summary>
        /// A single candidate selection for one slot. Price is the total spend
        /// (for Fan this is price × quantity). All component fields default to null.
        /// </summary>
        private sealed record Candidate(
            decimal Price,
            Motherboard? Mb = null,
            Ram? Ram = null,
            CpuCooler? Cooler = null,
            Ssd? Ssd = null,
            Hdd? Hdd = null,
            PcCase? PcCase = null,
            Fan? Fan = null,
            int FanQuantity = 0,
            PowerSupply? Psu = null);
    }
}
