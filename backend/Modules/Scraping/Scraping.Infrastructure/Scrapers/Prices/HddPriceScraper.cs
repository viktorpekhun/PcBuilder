using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class HddPriceScraper : HotlinePriceScraperBase<Hdd>
    {
        protected override ComponentType ComponentType => ComponentType.Hdd;
    }
}
