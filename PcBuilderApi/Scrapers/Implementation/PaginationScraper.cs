using HtmlAgilityPack;

namespace PcBuilderApi.Scrapers.Implementation
{
    public class PaginationScraper : IPaginationScraper
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://hotline.ua"; // Замінити на актуальну базову URL-адресу сайту

        public PaginationScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetComponentLinksAsync(string categoryUrl)
        {
            var componentLinks = new List<string>();
            int currentPage = 1;

            while (true)
            {
                string pageUrl = $"{categoryUrl}?p={currentPage}";
                var pageContent = await _httpClient.GetStringAsync(pageUrl);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(pageContent);

                // Знаходимо всі посилання на товари
                var links = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'list-item__title-container')]//a")
                    ?.Select(node => node.GetAttributeValue("href", ""))
                    .Where(link => !string.IsNullOrEmpty(link))
                    .Select(link => BASE_URL + link) // Додаємо базову URL-адресу
                    .Distinct()
                    .ToList();

                if (links == null || links.Count == 0)
                    break;

                componentLinks.AddRange(links);
                currentPage++;
                await Task.Delay(2000);
            }

            return componentLinks;
        }
    }
}
