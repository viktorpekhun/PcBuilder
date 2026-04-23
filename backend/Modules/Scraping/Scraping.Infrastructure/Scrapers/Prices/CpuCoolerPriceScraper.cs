using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public class CpuCoolerPriceScraper : HotlinePriceScraperBase<CpuCooler>
    {
        protected override ComponentType ComponentType => ComponentType.CpuCooler;
    }
}
