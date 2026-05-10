using Microsoft.Extensions.Logging;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.AutoBuilder.Models;
using PcBuilds.Application.Compatibility;
using PcBuilds.Application.Compatibility.Internal;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Infrastructure.AutoBuilder
{
    public class GreedyAutoBuilderService : IAutoBuilderService
    {
        private static readonly HashSet<string> ValidFormFactors = new(StringComparer.OrdinalIgnoreCase)
            { "ATX", "Micro-ATX", "Mini-ITX", "E-ATX" };

        // Stop collecting once we have this many valid builds — first N found are already the
        // most expensive (price-desc iteration), so more would only be cheaper alternatives.
        private const int TargetBuilds = 5;

        private readonly IScenarioPolicyProvider _policyProvider;
        private readonly ICandidatePruner _pruner;
        private readonly IBuildAssembler _assembler;
        private readonly CompatibilityChecker _checker;
        private readonly IPcBuildGalleryMapper _mapper;
        private readonly ILogger<GreedyAutoBuilderService> _logger;

        public GreedyAutoBuilderService(
            IScenarioPolicyProvider policyProvider,
            ICandidatePruner pruner,
            IBuildAssembler assembler,
            CompatibilityChecker checker,
            IPcBuildGalleryMapper mapper,
            ILogger<GreedyAutoBuilderService> logger)
        {
            _policyProvider = policyProvider;
            _pruner = pruner;
            _assembler = assembler;
            _checker = checker;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<AutoBuildResultDto>> BuildAsync(AutoBuildRequestDto request, CancellationToken ct)
        {
            // Step A: validate
            if (request.PreferredFormFactor is not null && !ValidFormFactors.Contains(request.PreferredFormFactor))
                return Result.Failure<AutoBuildResultDto>(new Error(
                    IssueCodes.InvalidFormFactor,
                    $"Invalid form factor '{request.PreferredFormFactor}'. Valid values: ATX, Micro-ATX, Mini-ITX, E-ATX.",
                    400));

            var policy = _policyProvider.GetPolicy(request.PreferredUse);

            _logger.LogInformation("AutoBuild start: budget={Budget}, scenario={Scenario}, strictBudget{SB}, futureProof={FP}, formFactor={FF}",
                request.Budget, request.PreferredUse, request.IsStrictBudget, request.IsFutureProof, request.PreferredFormFactor ?? "(any)");

            // Step B: prune candidates (price-desc for CPU/GPU so backtracking tries best first)
            var pool = await _pruner.PruneAsync(request, policy, ct);

            _logger.LogInformation("Pool sizes: CPU={Cpu}, GPU={Gpu}, MB={Mb}, Cooler={Cooler}, PSU={Psu}, Case={Case}, RAM={Ram}, SSD={Ssd}, HDD={Hdd}, Fan={Fan}",
                pool.Cpus.Count, pool.Gpus.Count, pool.Motherboards.Count, pool.CpuCoolers.Count,
                pool.PowerSupplies.Count, pool.PcCases.Count, pool.Rams.Count, pool.Ssds.Count, pool.Hdds.Count, pool.Fans.Count);

            if (pool.Cpus.Count == 0 || pool.Gpus.Count == 0
                || pool.Motherboards.Count == 0 || pool.PowerSupplies.Count == 0
                || pool.PcCases.Count == 0 || pool.Rams.Count == 0
                || pool.Ssds.Count == 0)
            {
                _logger.LogWarning("Budget too low — empty slot in pool");
                return Result.Failure<AutoBuildResultDto>(new Error(
                    IssueCodes.BudgetTooLowForScenario,
                    "Budget cannot satisfy the scenario's minimum components.",
                    400));
            }

            // Step C: backtrack through CPU × GPU combinations (most expensive first).
            // For each pair that passes the budget and balance gates, attempt a full assembly.
            // Stop as soon as TargetBuilds valid builds are found — they are already the most
            // expensive ones because we iterate price-desc.
            var ctx = new AssemblyContext(request, policy, pool);
            var balance = policy.Balance;
            // Allow CPU+GPU to consume up to their combined allocation × 1.3 so the remaining
            // budget can cover the other slots.
            var maxCombined = request.Budget * (decimal)(policy.Allocation.Cpu + policy.Allocation.Gpu) * 1.3m;
            var scoredBuilds = new List<ScoredBuild>();

            foreach (var cpu in pool.Cpus)
            {
                ct.ThrowIfCancellationRequested();
                if (scoredBuilds.Count >= TargetBuilds) break;

                foreach (var gpu in pool.Gpus)
                {
                    ct.ThrowIfCancellationRequested();
                    if (scoredBuilds.Count >= TargetBuilds) break;

                    var combinedPrice = cpu.AveragePrice!.Value + gpu.AveragePrice!.Value;
                    if (combinedPrice > maxCombined) continue;

                    // Bottleneck balance gate — skip extreme CPU-GPU mismatches
                    if (cpu.PassMarkScore > 0 && gpu.PassMarkScore > 0)
                    {
                        var r = gpu.PassMarkScore / (cpu.PassMarkScore * CpuGpuBottleneckConstants.K);
                        if (r < balance.Min || r > balance.Max) continue;
                    }

                    var core = new CorePairing(cpu, gpu, combinedPrice, 0);
                    var build = await _assembler.TryAssembleAsync(core, ctx, ct);
                    if (build == null)
                    {
                        _logger.LogDebug("({Cpu} + {Gpu}) — assembly failed", cpu.Name, gpu.Name);
                        continue;
                    }

                    var ruleResults = _checker.CheckAll(build);
                    var report = BuildCompatibilityReport.From(ruleResults);
                    if (!report.IsStrictlyCompatible)
                    {
                        var codes = string.Join(", ", ruleResults
                            .Where(r => !r.IsStrictlyCompatible)
                            .SelectMany(r => r.Issues.Where(i => i.Severity == CompatibilitySeverity.Critical))
                            .Select(i => i.Code));
                        _logger.LogWarning("({Cpu} + {Gpu}) — assembled but incompatible: [{Codes}]",
                            cpu.Name, gpu.Name, codes);
                        continue;
                    }

                    var price = ComputePrice(build);
                    _logger.LogInformation("({Cpu} + {Gpu}) — accepted, price={Price:F0} UAH, fitness={Fitness:F3}",
                        cpu.Name, gpu.Name, price, report.OverallFitnessScore);
                    scoredBuilds.Add(new ScoredBuild(build, report, price));
                }
            }

            if (scoredBuilds.Count == 0)
            {
                _logger.LogWarning("No strictly-compatible build assembled");
                return Result.Failure<AutoBuildResultDto>(new Error(
                    IssueCodes.NoCompatibleBuildFound,
                    "No strictly-compatible build could be assembled within the given budget.",
                    400));
            }

            // Pick the winner: best fitness score first; price breaks ties.
            var inBudget = scoredBuilds.Where(b => b.Price <= request.Budget).ToList();
            var winner = inBudget
                .OrderByDescending(b => b.Report.OverallFitnessScore)
                .ThenByDescending(b => b.Price)
                .FirstOrDefault();

            if (winner == null && !request.IsStrictBudget)
            {
                winner = scoredBuilds
                    .Where(b => b.Price <= request.Budget * 1.05m)
                    .OrderByDescending(b => b.Report.OverallFitnessScore)
                    .ThenByDescending(b => b.Price)
                    .FirstOrDefault();
            }

            if (winner == null)
                return Result.Failure<AutoBuildResultDto>(new Error(
                    IssueCodes.NoCompatibleBuildFound,
                    "No build within budget. Consider raising the budget or disabling strict budget mode.",
                    400));

            winner.Build.Price = winner.Price;

            return Result.Success(new AutoBuildResultDto(
                Build: _mapper.ToGalleryDto(winner.Build),
                TotalPrice: winner.Price,
                FitnessScore: winner.Report.OverallFitnessScore,
                CompatibilityReport: winner.Report,
                Components: ExtractComponentIds(winner.Build)));
        }

        private static SelectedComponentsDto ExtractComponentIds(PcBuild build)
        {
            var ram = build.PcBuild_Rams.FirstOrDefault();
            var ssd = build.PcBuild_Ssds.FirstOrDefault();
            var hdd = build.PcBuild_Hdds.FirstOrDefault();
            var fan = build.PcBuild_Fans.FirstOrDefault();

            return new SelectedComponentsDto(
                CpuId:          build.CpuId,
                GpuId:          build.GpuId,
                MotherboardId:  build.MotherboardId,
                CpuCoolerId:    build.CpuCoolerId,
                PowerSupplyId:  build.PowerSupplyId,
                PcCaseId:       build.PcCaseId,
                RamId:          ram?.RamId,
                RamQuantity:    ram?.Quantity ?? 0,
                SsdId:          ssd?.SsdId,
                SsdQuantity:    ssd?.Quantity ?? 0,
                HddId:          hdd?.HddId,
                HddQuantity:    hdd?.Quantity ?? 0,
                FanId:          fan?.FanId,
                FanQuantity:    fan?.Quantity ?? 0);
        }

        private static decimal ComputePrice(PcBuild build)
        {
            var total = 0m;
            if (build.Cpu?.AveragePrice.HasValue == true) total += build.Cpu.AveragePrice!.Value;
            if (build.Gpu?.AveragePrice.HasValue == true) total += build.Gpu.AveragePrice!.Value;
            if (build.Motherboard?.AveragePrice.HasValue == true) total += build.Motherboard.AveragePrice!.Value;
            if (build.CpuCooler?.AveragePrice.HasValue == true) total += build.CpuCooler.AveragePrice!.Value;
            if (build.PowerSupply?.AveragePrice.HasValue == true) total += build.PowerSupply.AveragePrice!.Value;
            if (build.PcCase?.AveragePrice.HasValue == true) total += build.PcCase.AveragePrice!.Value;
            foreach (var r in build.PcBuild_Rams)
                if (r.Ram?.AveragePrice.HasValue == true) total += r.Ram.AveragePrice!.Value * r.Quantity;
            foreach (var s in build.PcBuild_Ssds)
                if (s.Ssd?.AveragePrice.HasValue == true) total += s.Ssd.AveragePrice!.Value * s.Quantity;
            foreach (var h in build.PcBuild_Hdds)
                if (h.Hdd?.AveragePrice.HasValue == true) total += h.Hdd.AveragePrice!.Value * h.Quantity;
            foreach (var f in build.PcBuild_Fans)
                if (f.Fan?.AveragePrice.HasValue == true) total += f.Fan.AveragePrice!.Value * f.Quantity;
            return total;
        }
    }
}
