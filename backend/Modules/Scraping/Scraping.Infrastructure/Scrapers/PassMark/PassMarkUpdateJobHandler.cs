using Components.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcBuilder.SharedKernel.Persistence;
using Scraping.Application.Interfaces;
using System.Text.RegularExpressions;

namespace Scraping.Infrastructure.Scrapers.PassMark
{
    public class PassMarkUpdateJobHandler
    {
        private const int BatchSize = 500;

        // Matches compact GPU codes like GTX1660S, RTX5060TI, RX6700XT, as well as
        // spaced variants like "RTX 5060 Ti" or "GTX 1660 SUPER".
        private static readonly Regex GpuModelRegex = new(
            @"\b(GTX|RTX|RX)\s*(\d{3,4})\s*(SUPER|TI|XTX|XT|S)?\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Matches VRAM codes like O8G, O16G, 8G, 8GB, 16GB.
        private static readonly Regex GpuVramRegex = new(
            @"\bO?(\d{1,2})GB?\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IApplicationDbContext _context;
        private readonly ICpuPassMarkScraper _cpuScraper;
        private readonly IGpuPassMarkScraper _gpuScraper;
        private readonly IStringMatchingService _matcher;
        private readonly ILogger<PassMarkUpdateJobHandler> _logger;

        public PassMarkUpdateJobHandler(
            IApplicationDbContext context,
            ICpuPassMarkScraper cpuScraper,
            IGpuPassMarkScraper gpuScraper,
            IStringMatchingService matcher,
            ILogger<PassMarkUpdateJobHandler> logger)
        {
            _context = context;
            _cpuScraper = cpuScraper;
            _gpuScraper = gpuScraper;
            _matcher = matcher;
            _logger = logger;
        }

        public async Task<PassMarkPreviewResult> PreviewAsync(CancellationToken ct)
        {
            var cpuEntries = await _cpuScraper.FetchAllAsync(ct);
            var gpuEntries = await _gpuScraper.FetchAllAsync(ct);

            var cpuMatches = await CollectMatchesAsync<Cpu>(cpuEntries, ct);
            var gpuMatches = await CollectMatchesAsync<Gpu>(gpuEntries, ct);

            return new PassMarkPreviewResult(
                Cpu: cpuMatches,
                Gpu: gpuMatches,
                CpuStats: BuildStats(cpuMatches),
                GpuStats: BuildStats(gpuMatches));
        }

        private static PassMarkPreviewStats BuildStats(IReadOnlyList<PassMarkPreviewEntry> entries)
        {
            int matched = entries.Count(e => e.MatchedName != null);
            return new PassMarkPreviewStats(Total: entries.Count, Matched: matched, Unmatched: entries.Count - matched);
        }

        private async Task<IReadOnlyList<PassMarkPreviewEntry>> CollectMatchesAsync<TEntity>(
            IReadOnlyList<PassMarkEntry> entries,
            CancellationToken ct)
            where TEntity : class
        {
            var allRows = await _context.Set<TEntity>().ToListAsync(ct);
            var results = new List<PassMarkPreviewEntry>(allRows.Count);

            foreach (var row in allRows)
            {
                var name = GetName(row);
                if (name == null) continue;

                var query = GetQueryName<TEntity>(name);
                var match = _matcher.FindBestMatch(query, entries, e => e.Name);
                results.Add(new PassMarkPreviewEntry(
                    DbName: name,
                    MatchedName: match?.Value.Name,
                    PassMarkScore: match?.Value.Score,
                    FuzzyScore: match?.Score));
            }

            return results;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            _logger.LogInformation("PassMark update job started.");

            var cpuEntries = await _cpuScraper.FetchAllAsync(ct);
            var gpuEntries = await _gpuScraper.FetchAllAsync(ct);

            int cpuUpdated = await UpdateEntitiesAsync<Cpu>(cpuEntries, ct);
            int gpuUpdated = await UpdateEntitiesAsync<Gpu>(gpuEntries, ct);

            _logger.LogInformation("PassMark update complete. CPUs updated: {CpuUpdated}, GPUs updated: {GpuUpdated}.",
                cpuUpdated, gpuUpdated);
        }

        private async Task<int> UpdateEntitiesAsync<TEntity>(
            IReadOnlyList<PassMarkEntry> entries,
            CancellationToken ct)
            where TEntity : class
        {
            int updated = 0;
            int matched = 0;
            int processed = 0;

            var allRows = await _context.Set<TEntity>().ToListAsync(ct);

            foreach (var batch in allRows.Chunk(BatchSize))
            {
                foreach (var row in batch)
                {
                    var name = GetName(row);
                    if (name == null) continue;

                    processed++;
                    var query = GetQueryName<TEntity>(name);
                    var match = _matcher.FindBestMatch(query, entries, e => e.Name);
                    if (match == null)
                    {
                        _logger.LogDebug("No PassMark match for {Name} (score below threshold).", name);
                        continue;
                    }

                    matched++;
                    SetScore(row, match.Value.Score);
                    updated++;
                    _logger.LogDebug("Matched {Name} → PassMark score {Score} (fuzzy score {FuzzyScore}).",
                        name, match.Value.Score, match.Score);
                }

                await _context.SaveChangesAsync(ct);
            }

            double matchRate = processed > 0 ? (double)matched / processed * 100 : 0;
            _logger.LogInformation("{Type}: processed {Processed}, matched {Matched} ({Rate:F1}%).",
                typeof(TEntity).Name, processed, matched, matchRate);

            if (matchRate < 80 && processed > 10)
                _logger.LogWarning("{Type} match rate {Rate:F1}% is below 80% — review normalization rules.",
                    typeof(TEntity).Name, matchRate);

            return updated;
        }

        // For GPUs with manufacturer-specific names (e.g. "ASUS TUF-GTX1660S-O6G-GAMING"),
        // extract just the GPU model and VRAM so the fuzzy matcher can find "GeForce GTX 1660 SUPER".
        private static string GetQueryName<TEntity>(string name) =>
            typeof(TEntity) == typeof(Gpu) ? NormalizeGpuQuery(name) : name;

        private static string NormalizeGpuQuery(string name)
        {
            var text = name.Replace('-', ' ');

            var modelMatch = GpuModelRegex.Match(text);
            if (!modelMatch.Success)
                return name;

            var family = modelMatch.Groups[1].Value.ToUpperInvariant();
            var number = modelMatch.Groups[2].Value;
            var rawSuffix = modelMatch.Groups[3].Value.ToUpperInvariant();

            var suffix = rawSuffix switch
            {
                "S" or "SUPER" => " SUPER",
                "TI" => " Ti",
                "XT" => " XT",
                "XTX" => " XTX",
                _ => ""
            };

            var model = $"{family} {number}{suffix}";

            var vramMatch = GpuVramRegex.Match(text);
            var vram = vramMatch.Success ? $" {vramMatch.Groups[1].Value}GB" : "";

            return model + vram;
        }

        private static string? GetName<TEntity>(TEntity entity) => entity switch
        {
            Cpu cpu => cpu.Name,
            Gpu gpu => gpu.Name,
            _ => null
        };

        private static void SetScore<TEntity>(TEntity entity, int score)
        {
            switch (entity)
            {
                case Cpu cpu: cpu.PassMarkScore = score; break;
                case Gpu gpu: gpu.PassMarkScore = score; break;
            }
        }
    }

    public record PassMarkPreviewEntry(string DbName, string? MatchedName, int? PassMarkScore, int? FuzzyScore);
    public record PassMarkPreviewStats(int Total, int Matched, int Unmatched);
    public record PassMarkPreviewResult(
        IReadOnlyList<PassMarkPreviewEntry> Cpu,
        IReadOnlyList<PassMarkPreviewEntry> Gpu,
        PassMarkPreviewStats CpuStats,
        PassMarkPreviewStats GpuStats);
}
