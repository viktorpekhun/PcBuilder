using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class SsdPriceScraper : HotlinePriceScraperBase<Ssd>
    {
        protected override ComponentType ComponentType => ComponentType.Ssd;
    }
}
