namespace PcBuilderApi.Scrapers
{
    public interface IComponentScraper<TComponent> where TComponent : class
    {
        Task<ScrapingResult<TComponent>> ScrapeAsync(string url, HttpClient client);
    }
}
