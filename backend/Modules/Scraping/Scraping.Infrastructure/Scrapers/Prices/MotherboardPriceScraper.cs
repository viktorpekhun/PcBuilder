using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class MotherboardPriceScraper : HotlinePriceScraperBase<Motherboard>
    {
        protected override ComponentType ComponentType => ComponentType.Motherboard;
    }
}
