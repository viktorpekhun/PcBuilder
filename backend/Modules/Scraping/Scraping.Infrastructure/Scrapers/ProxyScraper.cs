using Scraping.Application.Interfaces;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Scraping.Infrastructure.Scrapers
{
    public class ProxyScraper : IProxyScraper
    {
        public async Task<List<string>> GetProxiesAsync(CancellationToken cancellationToken = default)
        {
            var allProxies = new List<string>();

            var tasks = new List<Task<List<string>>>
            {
                ScrapeProxyScrapeAsync(cancellationToken),
                ScrapeFreeProxyListAsync(cancellationToken),
                ScrapeGitHubListAsync("https://raw.githubusercontent.com/TheSpeedX/PROXY-List/master/http.txt", "TheSpeedX", cancellationToken),
                ScrapeGitHubListAsync("https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/http.txt", "monosans", cancellationToken),
                ScrapeGitHubListAsync("https://www.proxy-list.download/api/v1/get?type=http", "proxy-list.download", cancellationToken)
            };

            var results = await Task.WhenAll(tasks);
            foreach (var result in results)
            {
                allProxies.AddRange(result);
            }

            return allProxies.Distinct().ToList();
        }

        private static async Task<List<string>> ScrapeProxyScrapeAsync(CancellationToken cancellationToken)
        {
            var proxies = new List<string>();
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                string content = await client.GetStringAsync(
                    "https://api.proxyscrape.com/v2/?request=displayproxies&protocol=http&timeout=10000&country=all&ssl=all&anonymity=anonymous",
                    cancellationToken);

                var regex = new Regex(@"\d{1,3}(\.\d{1,3}){3}:\d+");
                var matches = regex.Matches(content);

                proxies.AddRange(matches.Select(m => m.Value));
                Console.WriteLine($"Завантажено {proxies.Count} проксі з ProxyScrape");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка ProxyScrape: {ex.Message}");
            }

            return proxies;
        }

        private static async Task<List<string>> ScrapeFreeProxyListAsync(CancellationToken cancellationToken)
        {
            var proxies = new List<string>();
            try
            {
                using var client = new HttpClient();
                string content = await client.GetStringAsync("https://free-proxy-list.net/", cancellationToken);

                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var rows = doc.DocumentNode.SelectNodes("//table[contains(@class, 'table')]/tbody/tr");
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells == null || cells.Count < 7)
                            continue;

                        string anonymity = cells[4].InnerText.Trim().ToLowerInvariant();
                        if (anonymity != "anonymous" && anonymity != "elite proxy")
                            continue;

                        string ip = cells[0].InnerText.Trim();
                        string port = cells[1].InnerText.Trim();
                        proxies.Add($"{ip}:{port}");
                    }
                }

                Console.WriteLine($"Завантажено {proxies.Count} проксі з free-proxy-list.net");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка free-proxy-list.net: {ex.Message}");
            }

            return proxies;
        }

        private static async Task<List<string>> ScrapeGitHubListAsync(string url, string sourceName, CancellationToken cancellationToken)
        {
            var proxies = new List<string>();
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                string content = await client.GetStringAsync(url, cancellationToken);

                var regex = new Regex(@"\d{1,3}(\.\d{1,3}){3}:\d+");
                var matches = regex.Matches(content);

                proxies.AddRange(matches.Select(m => m.Value));
                Console.WriteLine($"Завантажено {proxies.Count} проксі з {sourceName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка {sourceName}: {ex.Message}");
            }

            return proxies;
        }
    }
}
