
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Scrapers
{
    public interface IPaginationScraper
    {
        /// <summary>
        /// Отримує список посилань на всі компоненти певного типу.
        /// </summary>
        /// <param name="categoryUrl">URL категорії (наприклад, процесори або відеокарти).</param>
        /// <returns>Список URL на кожен компонент.</returns>
        Task<List<string>> GetComponentLinksAsync(string categoryUrl);
    }
}
