namespace PcBuilderApi.Scrapers
{
    public interface IProxyScraper
    {
        Task<List<string>> GetProxiesAsync();
    }
}
