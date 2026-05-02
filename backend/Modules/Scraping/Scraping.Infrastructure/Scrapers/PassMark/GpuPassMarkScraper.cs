using Microsoft.Extensions.Logging;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Scrapers.PassMark
{
    public class GpuPassMarkScraper : PassMarkScraperBase, IGpuPassMarkScraper
    {
        public GpuPassMarkScraper(HttpClient http, ILogger<GpuPassMarkScraper> logger)
            : base(http, logger) { }

        protected override string Url => "https://www.videocardbenchmark.net/gpu_list.php";
        protected override string RowIdPrefix => "gpu";
        protected override string EntityKind => "GPU";
    }
}
