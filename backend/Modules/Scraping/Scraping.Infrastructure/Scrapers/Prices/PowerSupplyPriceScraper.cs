using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class PowerSupplyPriceScraper : HotlinePriceScraperBase<PowerSupply>
    {
        protected override ComponentType ComponentType => ComponentType.PowerSupply;
    }
}
