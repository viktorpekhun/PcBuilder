using Microsoft.Extensions.DependencyInjection;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Scrapers
{
    public class PriceScraperFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PriceScraperFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPriceScraper<T>? GetScraper<T>() where T : class
        {
            return _serviceProvider.GetService<IPriceScraper<T>>();
        }
    }
}
