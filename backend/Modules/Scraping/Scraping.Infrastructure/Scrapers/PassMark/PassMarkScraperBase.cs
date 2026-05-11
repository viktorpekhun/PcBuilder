using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Scraping.Application.Interfaces;
using Scraping.Infrastructure.Utilities;

namespace Scraping.Infrastructure.Scrapers.PassMark
{
    public abstract class PassMarkScraperBase
    {
        private const int MaxAttempts = 3;
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(60);

        private readonly HttpClient _http;
        private readonly ILogger _logger;

        protected PassMarkScraperBase(HttpClient http, ILogger logger)
        {
            _http = http;
            _logger = logger;
            _http.Timeout = RequestTimeout;
        }

        protected abstract string Url { get; }
        protected abstract string RowIdPrefix { get; }
        protected abstract string EntityKind { get; }

        // Return null to discard the row, or return the (possibly modified) name to keep it.
        protected virtual string? TransformName(string name) => name;

        public async Task<IReadOnlyList<PassMarkEntry>> FetchAllAsync(CancellationToken ct)
        {
            var html = await FetchHtmlWithRetryAsync(ct);
            var entries = ParseRows(html);

            _logger.LogInformation("PassMark {Kind}: parsed {Count} entries from {Url}.",
                EntityKind, entries.Count, Url);

            if (entries.Count < 100)
                _logger.LogWarning("PassMark {Kind}: only {Count} entries parsed — markup may have changed.",
                    EntityKind, entries.Count);

            return entries;
        }

        private async Task<string> FetchHtmlWithRetryAsync(CancellationToken ct)
        {
            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Get, Url);
                    req.Headers.UserAgent.ParseAdd(UserAgentRotator.GetRandom());
                    req.Headers.Accept.ParseAdd("text/html,application/xhtml+xml");

                    using var resp = await _http.SendAsync(req, ct);
                    resp.EnsureSuccessStatusCode();
                    return await resp.Content.ReadAsStringAsync(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex) when (attempt < MaxAttempts)
                {
                    var delay = TimeSpan.FromSeconds(2 * attempt);
                    _logger.LogWarning(ex, "PassMark {Kind} fetch attempt {Attempt}/{Max} failed; retrying in {Delay}s.",
                        EntityKind, attempt, MaxAttempts, delay.TotalSeconds);
                    await Task.Delay(delay, ct);
                }
            }
            throw new InvalidOperationException($"PassMark {EntityKind} fetch failed after {MaxAttempts} attempts.");
        }

        private List<PassMarkEntry> ParseRows(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rows = doc.DocumentNode.SelectNodes($"//tr[starts-with(@id, '{RowIdPrefix}')]");
            if (rows == null) return new List<PassMarkEntry>();

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<PassMarkEntry>(rows.Count);

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("./td");
                if (cells == null || cells.Count < 2) continue;

                var nameNode = cells[0].SelectSingleNode(".//a") ?? cells[0];
                var rawName = HtmlEntity.DeEntitize(nameNode.InnerText).Trim();
                if (string.IsNullOrEmpty(rawName)) continue;
                var name = TransformName(rawName);
                if (name == null) continue;

                var scoreText = cells[1].InnerText.Trim().Replace(",", "").Replace(" ", "");
                if (!int.TryParse(scoreText, out var score) || score <= 0) continue;

                if (seen.Add(name))
                    results.Add(new PassMarkEntry(name, score));
            }

            return results;
        }
    }
}
