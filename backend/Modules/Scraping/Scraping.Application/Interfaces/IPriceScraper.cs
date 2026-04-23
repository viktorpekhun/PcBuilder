using Components.Domain.Entities;
using System.Collections.Concurrent;

namespace Scraping.Application.Interfaces
{
    public interface IPriceScraper<TComponent> where TComponent : class
    {
        Task<PriceScrapingResult> ScrapeAsync(
            string url,
            HttpClient client,
            Guid componentId,
            ConcurrentBag<Store> storesFromDb,
            CancellationToken cancellationToken = default);
    }

    public sealed record PriceScrapingResult(List<Store> Stores, List<ProductOffer> Offers);
}
