using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class RamPriceScraper : HotlinePriceScraperBase<Ram>
    {
        protected override ComponentType ComponentType => ComponentType.Ram;
    }
}
