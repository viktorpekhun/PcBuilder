namespace Scraping.Application.Interfaces
{
    public interface IPaginationScraper
    {
        Task<List<string>> GetComponentLinksAsync(string categoryUrl, CancellationToken cancellationToken = default);
    }
}
