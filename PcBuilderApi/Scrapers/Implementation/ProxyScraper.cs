using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace PcBuilderApi.Scrapers.Implementation
{
    public class ProxyScraper : IProxyScraper
    {
        public async Task<List<string>> GetProxiesAsync()
        {
            var allProxies = new List<string>();

            var spysProxies = await ScrapeSpysMeAsync();
            var freeListProxies = await ScrapeFreeProxyListAsync();

            allProxies.AddRange(spysProxies);
            allProxies.AddRange(freeListProxies);

            return allProxies.Distinct().ToList();
        }

        private async Task<List<string>> ScrapeSpysMeAsync()
        {
            var proxies = new List<string>();
            try
            {
                using HttpClient client = new HttpClient();
                string content = await client.GetStringAsync("https://spys.me/proxy.txt");

                var regex = new Regex(@"[0-9]+(?:\.[0-9]+){3}:[0-9]+");
                var matches = regex.Matches(content);

                proxies.AddRange(matches.Select(m => m.Value));
                Console.WriteLine("🔹 Завантажено проксі з spys.me");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Помилка spys.me: " + ex.Message);
            }

            return proxies;
        }

        private async Task<List<string>> ScrapeFreeProxyListAsync()
        {
            var proxies = new List<string>();
            try
            {
                using HttpClient client = new HttpClient();
                string content = await client.GetStringAsync("https://free-proxy-list.net/");

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(content);

                var rows = doc.DocumentNode.SelectNodes("//table[contains(@class, 'table')]/tbody/tr");
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells != null && cells.Count >= 2)
                        {
                            string ip = cells[0].InnerText.Trim();
                            string port = cells[1].InnerText.Trim();
                            proxies.Add($"{ip}:{port}");
                        }
                    }
                }

                Console.WriteLine("🔹 Завантажено проксі з free-proxy-list.net");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Помилка free-proxy-list.net: " + ex.Message);
            }

            return proxies;
        }
    }
}
