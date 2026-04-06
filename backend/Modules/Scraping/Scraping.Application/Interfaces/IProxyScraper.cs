namespace Scraping.Application.Interfaces
{
    public interface IProxyScraper
    {
        Task<List<string>> GetProxiesAsync(CancellationToken cancellationToken = default);
    }
}
