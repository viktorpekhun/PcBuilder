namespace Scraping.Application.Interfaces
{
    public record PassMarkEntry(string Name, int Score);

    public interface ICpuPassMarkScraper
    {
        Task<IReadOnlyList<PassMarkEntry>> FetchAllAsync(CancellationToken ct);
    }

    public interface IGpuPassMarkScraper
    {
        Task<IReadOnlyList<PassMarkEntry>> FetchAllAsync(CancellationToken ct);
    }
}
