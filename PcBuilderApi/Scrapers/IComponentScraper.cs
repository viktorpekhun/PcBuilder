namespace PcBuilderApi.Scrapers
{
    public interface IComponentScraper<TComponent>
    {
        Task<TComponent?> ScrapeAsync(string url, HttpClient client);
    }
}
