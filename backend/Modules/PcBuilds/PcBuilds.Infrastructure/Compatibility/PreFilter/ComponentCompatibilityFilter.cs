using Components.Application.PreFilter;
using Components.Domain.Entities;
using PcBuilds.Application.Compatibility.PreFilter;

namespace PcBuilds.Infrastructure.Compatibility.PreFilter
{
    public class ComponentCompatibilityFilter : IComponentCompatibilityFilter, IComponentQueryPreFilter
    {
        // ── IComponentQueryPreFilter bridge (Components.Application context) ──────

        public IQueryable<Cpu>         FilterCpus         (IQueryable<Cpu> q,         PartialBuildContext ctx) => FilterCpus(q, ToPartialBuild(ctx));
        public IQueryable<Gpu>         FilterGpus         (IQueryable<Gpu> q,         PartialBuildContext ctx) => FilterGpus(q, ToPartialBuild(ctx));
        public IQueryable<Motherboard> FilterMotherboards (IQueryable<Motherboard> q, PartialBuildContext ctx) => FilterMotherboards(q, ToPartialBuild(ctx));
        public IQueryable<Ram>         FilterRams         (IQueryable<Ram> q,         PartialBuildContext ctx) => FilterRams(q, ToPartialBuild(ctx));
        public IQueryable<CpuCooler>   FilterCoolers      (IQueryable<CpuCooler> q,   PartialBuildContext ctx) => FilterCoolers(q, ToPartialBuild(ctx));
        public IQueryable<PcCase>      FilterCases        (IQueryable<PcCase> q,      PartialBuildContext ctx) => FilterCases(q, ToPartialBuild(ctx));
        public IQueryable<PowerSupply> FilterPowerSupplies(IQueryable<PowerSupply> q, PartialBuildContext ctx) => FilterPowerSupplies(q, ToPartialBuild(ctx));
        public IQueryable<Fan>         FilterFans         (IQueryable<Fan> q,         PartialBuildContext ctx) => FilterFans(q, ToPartialBuild(ctx));

        private static PartialBuild ToPartialBuild(PartialBuildContext ctx) =>
            new(ctx.Cpu, ctx.Gpu, ctx.Motherboard, ctx.Ram, ctx.CpuCooler, ctx.PcCase, ctx.PowerSupply);

        // ── SQL-translatable filter methods ───────────────────────────────────────

        public IQueryable<Cpu> FilterCpus(IQueryable<Cpu> q, PartialBuild ctx)
        {
            if (ctx.Motherboard is { } mb)
            {
                if (string.IsNullOrEmpty(mb.Socket))
                    return q.Where(_ => false);
                q = q.Where(c => c.Socket == mb.Socket);
            }
            return q;
        }

        public IQueryable<Gpu> FilterGpus(IQueryable<Gpu> q, PartialBuild ctx)
        {
            if (ctx.PcCase is { } pcCase)
            {
                if (pcCase.MaxGpuLength == null)
                    return q.Where(_ => false);
                q = q.Where(g => g.SizeLength == null || g.SizeLength <= pcCase.MaxGpuLength);
            }

            if (ctx.Motherboard is { } mb)
            {
                if (!mb.PcleSlots.Any())
                    return q.Where(_ => false);
                // SQL-translatable via local scalar: GPU's required PCIe version must be ≤ the MB's best slot.
                // Full per-slot lane check is not EF-translatable with a local collection; version covers the reported warning.
                double mbMaxVersion = mb.PcleSlots.Max(s => s.Version);
                q = q.Where(g => !g.PcleVersion.HasValue || g.PcleVersion <= mbMaxVersion);
            }

            return q;
        }

        public IQueryable<Motherboard> FilterMotherboards(IQueryable<Motherboard> q, PartialBuild ctx)
        {
            if (ctx.Cpu is { } cpu)
            {
                if (string.IsNullOrEmpty(cpu.Socket))
                    return q.Where(_ => false);
                q = q.Where(m => m.Socket == cpu.Socket);
            }

            if (ctx.Ram is { } ram)
            {
                // RAM.Type is non-nullable string (default "")
                if (string.IsNullOrEmpty(ram.Type))
                    return q.Where(_ => false);
                q = q.Where(m => m.DimmType == ram.Type);

                // MB must support the RAM's frequency — null MB freq means unknown (no data)
                q = q.Where(m => m.DimmFrequency != null && m.DimmFrequency >= ram.Frequency);
            }

            if (ctx.PcCase is { } pcCase)
            {
                var validFormFactors = pcCase.PcCaseFormFactors.Select(ff => ff.Name).ToList();
                if (!validFormFactors.Any())
                    return q.Where(_ => false);
                q = q.Where(m => m.FormFactor == null || validFormFactors.Contains(m.FormFactor));
            }

            if (ctx.Gpu is { } gpu)
            {
                if (!gpu.PcleVersion.HasValue)
                    return q.Where(_ => false);
                var gpuVer = gpu.PcleVersion.Value;
                var gpuLane = gpu.PcleLane;
                // Mirrors GpuMotherboardPcleRule: clean pass requires a slot with Version >= GPU's version
                // AND Lane >= GPU's lane. Null lane treated as no constraint (data quality issue, not incompatibility).
                q = q.Where(m => m.PcleSlots.Any(s =>
                    s.Version >= gpuVer &&
                    (gpuLane == null || s.Lane == null || s.Lane >= gpuLane)));
            }

            return q;
        }

        public IQueryable<Ram> FilterRams(IQueryable<Ram> q, PartialBuild ctx)
        {
            if (ctx.Motherboard is { } mb)
            {
                if (string.IsNullOrEmpty(mb.DimmType))
                    return q.Where(_ => false);
                q = q.Where(r => r.Type == mb.DimmType);

                if (mb.DimmFrequency.HasValue)
                    q = q.Where(r => r.Frequency <= mb.DimmFrequency.Value);
            }
            return q;
        }

        public IQueryable<CpuCooler> FilterCoolers(IQueryable<CpuCooler> q, PartialBuild ctx)
        {
            if (ctx.Motherboard is { } mb)
            {
                if (string.IsNullOrEmpty(mb.Socket))
                    return q.Where(_ => false);

                var mbSocket = mb.Socket;
                // Mirror CpuCoolerMotherboardSocketRule: Socket 115x family treated as compatible
                q = q.Where(c =>
                    c.CpuCoolerSockets.Any(s =>
                        s.SocketType == mbSocket ||
                        (mbSocket.Contains("Socket 115") && s.SocketType.Contains("Socket 115"))));
            }

            if (ctx.Cpu is { } cpu && cpu.Tdp.HasValue)
            {
                var tdp = cpu.Tdp.Value;
                q = q.Where(c => c.MaxPowerDissipation == null || c.MaxPowerDissipation >= tdp);
            }

            if (ctx.PcCase is { } pcCase && pcCase.MaxCpuCoolerHeight.HasValue)
            {
                var maxHeight = pcCase.MaxCpuCoolerHeight.Value;
                // Air coolers: height must fit. Water coolers are not height-constrained here.
                q = q.Where(c => c.Type != "Air" || c.Height == null || c.Height <= maxHeight);
            }

            return q;
        }

        public IQueryable<PcCase> FilterCases(IQueryable<PcCase> q, PartialBuild ctx)
        {
            if (ctx.Motherboard is { } mb && !string.IsNullOrEmpty(mb.FormFactor))
            {
                var ff = mb.FormFactor;
                q = q.Where(c => c.PcCaseFormFactors.Any(f => f.Name == ff));
            }

            if (ctx.Gpu is { } gpu && gpu.SizeLength.HasValue)
            {
                var gpuLen = gpu.SizeLength.Value;
                q = q.Where(c => c.MaxGpuLength == null || c.MaxGpuLength >= gpuLen);
            }

            if (ctx.CpuCooler is { } cooler && cooler.Type == "Air" && cooler.Height.HasValue)
            {
                var coolerH = cooler.Height.Value;
                q = q.Where(c => c.MaxCpuCoolerHeight == null || c.MaxCpuCoolerHeight >= coolerH);
            }

            return q;
        }

        public IQueryable<PowerSupply> FilterPowerSupplies(IQueryable<PowerSupply> q, PartialBuild ctx)
        {
            // SQL-translatable only. Connector pin-matching and wattage estimation
            // are handled in ApplyInMemoryPsuRules (used by the autobuilder).
            // No constraints can be expressed as simple SQL for PSUs given the current
            // partial build shape — this method is a no-op placeholder for composability.
            return q;
        }

        public IQueryable<Fan> FilterFans(IQueryable<Fan> q, PartialBuild ctx)
        {
            if (ctx.PcCase is { } pcCase)
            {
                if (!pcCase.PcCaseFanLocations.Any())
                    return q.Where(_ => false);

                var validSizes = pcCase.PcCaseFanLocations.Select(fl => fl.FanSize).Distinct().ToList();
                q = q.Where(f => f.SizeLength != null && validSizes.Contains((int)f.SizeLength.Value));
            }
            return q;
        }

        public IQueryable<Ssd> FilterSsds(IQueryable<Ssd> q, PartialBuild ctx)
        {
            if (ctx.Motherboard is { } mb)
            {
                bool mbHasM2 = mb.M2Slots != null && mb.M2Slots.Any();
                bool mbHasSata = mb.Sata3Count != null && mb.Sata3Count > 0;

                // Interface naming convention: contains "M.2" for NVMe / "SATA" for SATA SSDs.
                // Unknown / missing interface data is treated as compatible (lets the rule's
                // warning-level handling apply rather than excluding silently).
                q = q.Where(s =>
                    s.Interface == null
                    || (!s.Interface.Contains("M.2") || mbHasM2)
                );
                q = q.Where(s =>
                    s.Interface == null
                    || (!s.Interface.Contains("SATA") || mbHasSata)
                );
            }
            return q;
        }

        public IQueryable<Hdd> FilterHdds(IQueryable<Hdd> q, PartialBuild ctx)
        {
            if (ctx.Motherboard is { } mb)
            {
                // HDDs are always SATA. The Hdd-MB rule treats Sata3Count == 0 as Critical,
                // so this is a hard filter (no fallback).
                if (mb.Sata3Count == null || mb.Sata3Count == 0)
                    return q.Where(_ => false);
            }
            return q;
        }

        // ── In-memory PSU rules ───────────────────────────────────────────────────

        public IEnumerable<PowerSupply> ApplyInMemoryPsuRules(IEnumerable<PowerSupply> psus, PartialBuild ctx)
        {
            int required = EstimateRequiredWattage(ctx);
            // 3% headroom for transient spikes; storage/fan wattages are now included in the estimate
            int minWattage = (int)(required * 1.03);

            return psus.Where(psu =>
                psu.Wattage >= minWattage
                && CanSatisfyCpuConnectors(psu, ctx.Motherboard)
                && CanSatisfyGpuConnectors(psu, ctx.Gpu));
        }

        // ── Private helpers (mirrors BuildAssembler logic exactly) ────────────────

        /// <summary>
        /// Mirrors BuildAssembler.EstimateRequiredWattage using only the components
        /// present in the partial build. Missing components contribute zero.
        /// </summary>
        private static int EstimateRequiredWattage(PartialBuild ctx)
        {
            int total = 0;

            if (ctx.Cpu != null)
                total += ctx.Cpu.Tdp ?? 0;

            if (ctx.Gpu != null)
                total += ctx.Gpu.Wattage ??
                    (ctx.Gpu.PsuReccomended.HasValue ? (int)(ctx.Gpu.PsuReccomended.Value * 0.45) : 0);

            if (ctx.Motherboard != null)
                total += ctx.Motherboard.Wattage ?? 0;

            if (ctx.CpuCooler != null)
                total += ctx.CpuCooler.Wattage ?? 0;

            if (ctx.Ram != null)
            {
                int ramModuleQty = ctx.Ram.ModuleQuantity ?? 1;
                total += (ctx.Ram.Wattage ?? 0) * ramModuleQty;
            }

            if (ctx.Ssd is not null)
                total += ctx.Ssd.Wattage;

            if (ctx.Hdd is not null)
                total += ctx.Hdd.Wattage ?? 0;

            if (ctx.Fan is not null)
            {
                int fanModuleCount = ctx.Fan.ModuleCount ?? 1;
                total += (ctx.Fan.Wattage ?? 0) * fanModuleCount * Math.Max(1, ctx.FanQuantity);
            }

            total += 150; // flat overhead matching RequiredPowerSupplyRule

            return total;
        }

        /// <summary>
        /// Mirrors BuildAssembler.CanSatisfyCpuConnectors exactly.
        /// Returns true when mb is null (no constraint to check).
        /// </summary>
        private static bool CanSatisfyCpuConnectors(PowerSupply psu, Motherboard? mb)
        {
            if (mb == null) return true;

            var required = mb.CpuPowerConnectors;
            var provided = psu.PowerSupplyPowerConnectors;

            if (required == null || !required.Any() || provided == null || !provided.Any())
                return true;

            var cpuProvided = provided.Where(c => c.Type == "CPU").ToList();
            if (!cpuProvided.Any()) return false;

            var available = cpuProvided
                .Select(c => new { c.Pins, c.AdditionalPins, Remaining = c.Quantity })
                .ToList();

            foreach (var grp in required.GroupBy(c => c.Pins))
            {
                int pinCount = grp.Key;
                int needed = grp.Sum(c => c.Quantity);
                int satisfied = 0;

                for (int i = 0; i < available.Count && satisfied < needed; i++)
                {
                    var conn = available[i];
                    int used = 0;

                    if (conn.Pins == pinCount)
                        used = Math.Min(conn.Remaining, needed - satisfied);
                    else if (conn.Pins + (conn.AdditionalPins ?? 0) == pinCount)
                        used = Math.Min(conn.Remaining, needed - satisfied);
                    else if (conn.Pins == pinCount * 2 && (conn.AdditionalPins ?? 0) == 0)
                        used = Math.Min(conn.Remaining * 2, needed - satisfied);
                    else if (conn.Pins == pinCount && conn.AdditionalPins > 0)
                        used = Math.Min(conn.Remaining * 2, needed - satisfied);

                    satisfied += used;
                    available[i] = new { conn.Pins, conn.AdditionalPins, Remaining = conn.Remaining - (used / 2 + used % 2) };
                }

                if (satisfied < needed) return false;
            }

            return true;
        }

        /// <summary>
        /// Mirrors BuildAssembler.CanSatisfyGpuConnectors exactly.
        /// Returns true when gpu is null (no constraint to check).
        /// </summary>
        private static bool CanSatisfyGpuConnectors(PowerSupply psu, Gpu? gpu)
        {
            if (gpu == null) return true;

            var required = gpu.GpuPowerConnectors;
            var provided = psu.PowerSupplyPowerConnectors;

            if (required == null || !required.Any() || provided == null || !provided.Any())
                return true;

            var gpuProvided = provided.Where(c => c.Type == "GPU").ToList();
            if (!gpuProvided.Any()) return false;

            var available = gpuProvided
                .Select(c => new { c.Pins, c.AdditionalPins, Remaining = c.Quantity })
                .ToList();

            foreach (var grp in required.GroupBy(c => c.Pins))
            {
                int pinCount = grp.Key;
                int needed = grp.Sum(c => c.Quantity);
                int satisfied = 0;

                for (int i = 0; i < available.Count && satisfied < needed; i++)
                {
                    var conn = available[i];
                    int used = 0;

                    if (conn.Pins == pinCount)
                        used = Math.Min(conn.Remaining, needed - satisfied);
                    else if (conn.Pins + (conn.AdditionalPins ?? 0) == pinCount)
                        used = Math.Min(conn.Remaining, needed - satisfied);
                    else if (conn.Pins + (conn.AdditionalPins ?? 0) >= pinCount && conn.Pins <= pinCount)
                        used = Math.Min(conn.Remaining, needed - satisfied);

                    satisfied += used;
                    available[i] = new { conn.Pins, conn.AdditionalPins, Remaining = conn.Remaining - used };
                }

                if (satisfied < needed) return false;
            }

            return true;
        }
    }
}
