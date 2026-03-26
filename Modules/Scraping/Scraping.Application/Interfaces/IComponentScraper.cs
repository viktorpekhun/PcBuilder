using Components.Domain.Entities;
using System.Collections.Concurrent;

namespace Scraping.Application.Interfaces
{
    public interface IComponentScraper<TComponent> where TComponent : class
    {
        Task<ScrapingResult<TComponent>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<TComponent> componentsFromDb, ConcurrentBag<Store> storesFromDb);
    }
}
