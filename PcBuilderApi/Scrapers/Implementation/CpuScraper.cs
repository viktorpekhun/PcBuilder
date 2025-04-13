using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PcBuilderApi.Models;

namespace PcBuilderApi.Scrapers.Implementation
{
    public class CpuScraper : IComponentScraper<Cpu>
    {
        private const string BaseUrl = "https://hotline.ua";

        public async Task<Cpu?> ScrapeAsync(string url, HttpClient client)
        {

            var html = await client.GetStringAsync(url);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var cpu = new Cpu();

            // Отримуємо назву моделі (та текст у дужках, якщо є)
            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                cpu.Name = titleNode.InnerText.Trim();
                var match = Regex.Match(cpu.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                cpu.Name = Regex.Replace(cpu.Name, @"\s*\(.*?\)", ""); // Видаляємо текст у дужках
            }
            else
            {
                return null;
            }

            // Отримуємо опис без тексту в дужках
            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description__content')]");
            if (descriptionNode != null)
            {
                cpu.Description = descriptionNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(modelInBrackets))
                {
                    cpu.Description = Regex.Replace(cpu.Description, $@"\({Regex.Escape(modelInBrackets)}\)", "");
                }
            }

            // Отримуємо характеристики
            var specTable = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'specifications-table')]");
            if (specTable != null)
            {
                foreach (var row in specTable.SelectNodes(".//tr"))
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 2) continue;

                    var key = ExtractText(cells[0]).Trim(':', ' ');
                    var value = ExtractText(cells[1]).Trim();

                    switch (key)
                    {
                        case "Бренд":
                            cpu.Brand = value;
                            break;
                        case "Тип роз'єму":
                            cpu.Socket = value;
                            break;
                        case "Базова частота продуктивних ядер, ГГц":
                            cpu.BasicFrequency = ParseDouble(value);
                            break;
                        case "Максимальна частота продуктивних ядер, ГГц":
                            cpu.MaxFrequency = ParseDouble(value);
                            break;
                        case "Об'єм кеш-пам'яті третього рівня, МБ":
                            cpu.Cache = ParseInt(value);
                            break;
                        case "Тип пам'яті":
                            cpu.DimmType = value;
                            break;
                        case "Загальна кількість ядер":
                            cpu.Cores = ParseInt(value);
                            break;
                        case "Кількість потоків":
                            cpu.Threads = ParseInt(value);
                            break;
                        case "Виробнича технологія, нм":
                            cpu.Techprocess = value;
                            break;
                        case "Базове тепловиділення TDP, Вт":
                            cpu.Tdp = ParseInt(value);
                            break;
                        case "Інтегрована графіка":
                            cpu.IntegratedGraphics = !value.Equals("немає", StringComparison.OrdinalIgnoreCase);
                            break;
                        case "Комплектація (Tray/Box/MPK)":
                            cpu.Complectation = value;
                            break;
                        case "Товар на сайті бренду":
                            {
                                var linkNode = cells[1].SelectSingleNode(".//a[@data-outer-link]");
                                if (linkNode != null)
                                {
                                    var outerLink = linkNode.GetAttributeValue("data-outer-link", string.Empty);
                                    if (!string.IsNullOrEmpty(outerLink))
                                    {
                                        cpu.FactoryLink = outerLink;
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            else
            {
                return null;
            }

            // Отримуємо URL зображення
            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                cpu.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
            }

            return cpu;
        }

        private string ExtractText(HtmlNode node) => node.SelectSingleNode(".//text()")?.InnerText.Trim() ?? "";

        private int? ParseInt(string value) => int.TryParse(value, out var result) ? result : null;

        private double? ParseDouble(string value)
        {
            value = value.Replace(',', '.');
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
        }
    }
}
