using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class CpuPriceScraper : HotlinePriceScraperBase<Cpu>
    {
        protected override ComponentType ComponentType => ComponentType.Cpu;
    }
}
