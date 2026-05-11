using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class PcCasePriceScraper : HotlinePriceScraperBase<PcCase>
    {
        protected override ComponentType ComponentType => ComponentType.PcCase;
    }
}
