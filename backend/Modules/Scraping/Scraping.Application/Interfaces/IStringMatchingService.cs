namespace Scraping.Application.Interfaces
{
    public interface IStringMatchingService
    {
        StringMatch<T>? FindBestMatch<T>(
            string query,
            IEnumerable<T> candidates,
            Func<T, string> selector,
            int minScore = 80);
    }

    public record StringMatch<T>(T Value, int Score);
}
