namespace Scraping.Application.Interfaces
{
    public interface ITranslationService
    {
        Task<string> TranslateAsync(string text, string from, string to);
        Task<IReadOnlyList<string>> TranslateBatchAsync(IReadOnlyList<string> texts, string from, string to);
    }
}
