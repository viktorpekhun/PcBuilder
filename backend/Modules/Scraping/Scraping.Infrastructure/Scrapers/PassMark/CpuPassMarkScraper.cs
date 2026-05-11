using Microsoft.Extensions.Logging;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Scrapers.PassMark
{
    public class CpuPassMarkScraper : PassMarkScraperBase, ICpuPassMarkScraper
    {
        public CpuPassMarkScraper(HttpClient http, ILogger<CpuPassMarkScraper> logger)
            : base(http, logger) { }

        protected override string Url => "https://www.cpubenchmark.net/cpu-list/all";
        protected override string RowIdPrefix => "cpu";
        protected override string EntityKind => "CPU";

        // Strip the clock-speed suffix ("@ 3.70GHz") so entries like
        // "Intel Pentium Gold G5400 @ 3.70GHz" become "Intel Pentium Gold G5400"
        // and can fuzzy-match our Cpu.Name. The dedup HashSet in the base class
        // ensures no duplicates when both the bare and clock-speed variants exist.
        protected override string? TransformName(string name)
        {
            var atIndex = name.IndexOf('@');
            return atIndex > 0 ? name[..atIndex].Trim() : name;
        }
    }
}
