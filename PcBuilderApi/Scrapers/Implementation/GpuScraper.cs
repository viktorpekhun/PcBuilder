using HtmlAgilityPack;
using PcBuilderApi.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PcBuilderApi.Scrapers.Implementation
{
    public class GpuScraper : IComponentScraper<Gpu>
    {
        private const string BaseUrl = "https://hotline.ua";

        public async Task<Gpu?> ScrapeAsync(string url, HttpClient client)
        {

            var html = await client.GetStringAsync(url);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var gpu = new Gpu();

            // Отримуємо назву моделі (та текст у дужках, якщо є)
            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                gpu.Name = titleNode.InnerText.Trim();
                var match = Regex.Match(gpu.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                gpu.Name = Regex.Replace(gpu.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return null;
            }

            // Отримуємо опис без тексту в дужках
            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description__content')]");
            if (descriptionNode != null)
            {
                gpu.Description = descriptionNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(modelInBrackets))
                {
                    gpu.Description = Regex.Replace(gpu.Description, $@"\({Regex.Escape(modelInBrackets)}\)", "");
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
                            gpu.Brand = value;
                            break;
                        case "Виробник GPU":
                            gpu.GpuManufacturer = value;
                            break;
                        case "Об'єм пам'яті, ГБ":
                            gpu.Memory = ParseInt(value);
                            break;
                        case "Тип пам'яті":
                            gpu.MemoryType = value;
                            break;
                        case "Інтерфейс":
                            var versionMatch = Regex.Match(value, @"PCI\s*Express\s*([\d.]+)");
                            if (versionMatch.Success && double.TryParse(versionMatch.Groups[1].Value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var version))
                            {
                                gpu.PcleVersion = version;
                            }

                            var laneMatch = Regex.Match(value, @"x(\d+)");
                            if (laneMatch.Success && int.TryParse(laneMatch.Groups[1].Value, out var lane))
                            {
                                gpu.PcleLane = lane;
                            }
                            break;
                        case "Максимальна частота роботи GPU, МГц":
                            gpu.MaxFrequency = ParseInt(value);
                            break;
                        case "Кількість CUDA ядер":
                            gpu.CudaCores = ParseInt(value);
                            break;
                        case "Швидкість пам'яті, Gbps":
                            gpu.MemorySpeed = ParseInt(value);
                            break;
                        case "Шина пам'яті, біт":
                            gpu.MemoryBus = ParseInt(value);
                            break;
                        case "Розміри, мм":
                            var normalized = value.Replace("х", "x").Replace("Х", "x").ToLower();
                            var matches = Regex.Matches(normalized, @"[\d.,]+");

                            if (matches.Count >= 1)
                                gpu.SizeLength = ParseDouble(matches[0].Value);
                            if (matches.Count >= 2)
                                gpu.SizeWidth = ParseDouble(matches[1].Value);
                            if (matches.Count >= 3)
                                gpu.SizeHeight = ParseDouble(matches[2].Value);

                            break;
                        case "Споживана потужність, Вт":
                            gpu.Wattage = ParseInt(value);
                            break;                        
                        case "Рекомендована потужність блоку живлення, Вт":
                            gpu.PsuReccomended = ParseInt(value);
                            break;
                        case "Додаткове живлення":
                            {
                                var connectors = new List<GpuPowerConnector>();

                                // Розбиваємо рядок на частини через "+" (наприклад: "2x8pin +1 x6pin")
                                var parts = value.Split('+', StringSplitOptions.RemoveEmptyEntries);

                                foreach (var part in parts)
                                {
                                    // Витягуємо кількість і кількість пінів — наприклад, з "2x8pin" отримаємо 2 та 8
                                    var match = Regex.Match(part.Trim(), @"(\d+)\s*x\s*(\d+)", RegexOptions.IgnoreCase);

                                    if (match.Success)
                                    {
                                        int quantity = int.Parse(match.Groups[1].Value);
                                        int pins = int.Parse(match.Groups[2].Value);

                                        connectors.Add(new GpuPowerConnector
                                        {
                                            Quantity = quantity,
                                            Pins = pins
                                            // GpuId буде встановлено пізніше при збереженні
                                        });
                                    }
                                }

                                gpu.GpuPowerConnectors = connectors;
                                break;
                            }
                        case "Товар на сайті бренду":
                            {
                                var linkNode = cells[1].SelectSingleNode(".//a[@data-outer-link]");
                                if (linkNode != null)
                                {
                                    var outerLink = linkNode.GetAttributeValue("data-outer-link", string.Empty);
                                    if (!string.IsNullOrEmpty(outerLink))
                                    {
                                        gpu.FactoryLink = outerLink;
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
                gpu.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
            }

            var productOffersNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'list-container')]");
            if (productOffersNode != null)
            {
                var lastNode = productOffersNode.SelectNodes(".//div").LastOrDefault();
                foreach (var offerNode in lastNode.SelectNodes(".//div"))
                {

                }
            }

            return gpu;
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

