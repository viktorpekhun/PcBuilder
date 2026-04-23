using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class GpuPriceScraper : HotlinePriceScraperBase<Gpu>
    {
        protected override ComponentType ComponentType => ComponentType.Gpu;
    }
}
