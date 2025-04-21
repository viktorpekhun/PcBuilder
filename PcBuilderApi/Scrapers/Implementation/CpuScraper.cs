//using System.Globalization;
//using System.Net.Http;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using HtmlAgilityPack;
//using PcBuilderApi.Models;

//namespace PcBuilderApi.Scrapers.Implementation
//{
//    public class CpuScraper : IComponentScraper<Cpu>
//    {
//        private const string BaseUrl = "https://hotline.ua";

//        public async Task<Cpu?> ScrapeAsync(string url, HttpClient client)
//        {

//            var html = await client.GetStringAsync(url);
//            var htmlDoc = new HtmlDocument();
//            htmlDoc.LoadHtml(html);

//            var cpu = new Cpu();

//            // Отримуємо назву моделі (та текст у дужках, якщо є)
//            string modelInBrackets = "";
//            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
//            if (titleNode != null)
//            {
//                cpu.Name = titleNode.InnerText.Trim();
//                var match = Regex.Match(cpu.Name, @"\((.*?)\)");
//                if (match.Success)
//                {
//                    modelInBrackets = match.Groups[1].Value;
//                }
//                cpu.Name = Regex.Replace(cpu.Name, @"\s*\(.*?\)", ""); // Видаляємо текст у дужках
//            }
//            else
//            {
//                return null;
//            }

//            // Отримуємо опис без тексту в дужках
//            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description__content')]");
//            if (descriptionNode != null)
//            {
//                cpu.Description = descriptionNode.InnerText.Trim();
//                if (!string.IsNullOrEmpty(modelInBrackets))
//                {
//                    cpu.Description = Regex.Replace(cpu.Description, $@"\({Regex.Escape(modelInBrackets)}\)", "");
//                }
//            }

//            // Отримуємо характеристики
//            var specTable = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'specifications-table')]");
//            if (specTable != null)
//            {
//                foreach (var row in specTable.SelectNodes(".//tr"))
//                {
//                    var cells = row.SelectNodes(".//td");
//                    if (cells == null || cells.Count < 2) continue;

//                    var key = ExtractText(cells[0]).Trim(':', ' ');
//                    var value = ExtractText(cells[1]).Trim();

//                    switch (key)
//                    {
//                        case "Бренд":
//                            cpu.Brand = value;
//                            break;
//                        case "Тип роз'єму":
//                            cpu.Socket = value;
//                            break;
//                        case "Базова частота продуктивних ядер, ГГц":
//                            cpu.BasicFrequency = ParseDouble(value);
//                            break;
//                        case "Максимальна частота продуктивних ядер, ГГц":
//                            cpu.MaxFrequency = ParseDouble(value);
//                            break;
//                        case "Об'єм кеш-пам'яті третього рівня, МБ":
//                            cpu.Cache = ParseInt(value);
//                            break;
//                        case "Тип пам'яті":
//                            cpu.DimmType = value;
//                            break;
//                        case "Загальна кількість ядер":
//                            cpu.Cores = ParseInt(value);
//                            break;
//                        case "Кількість потоків":
//                            cpu.Threads = ParseInt(value);
//                            break;
//                        case "Виробнича технологія, нм":
//                            cpu.Techprocess = value;
//                            break;
//                        case "Базове тепловиділення TDP, Вт":
//                            cpu.Tdp = ParseInt(value);
//                            break;
//                        case "Інтегрована графіка":
//                            cpu.IntegratedGraphics = !value.Equals("немає", StringComparison.OrdinalIgnoreCase);
//                            break;
//                        case "Комплектація (Tray/Box/MPK)":
//                            cpu.Complectation = value;
//                            break;
//                        case "Товар на сайті бренду":
//                            {
//                                var linkNode = cells[1].SelectSingleNode(".//a[@data-outer-link]");
//                                if (linkNode != null)
//                                {
//                                    var outerLink = linkNode.GetAttributeValue("data-outer-link", string.Empty);
//                                    if (!string.IsNullOrEmpty(outerLink))
//                                    {
//                                        cpu.FactoryLink = outerLink;
//                                    }
//                                }
//                                break;
//                            }
//                    }
//                }
//            }
//            else
//            {
//                return null;
//            }

//            // Отримуємо URL зображення
//            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
//            if (imageNode != null)
//            {
//                string imgSrc = imageNode.GetAttributeValue("src", "");
//                cpu.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
//            }

//            return cpu;
//        }

//        private string ExtractText(HtmlNode node) => node.SelectSingleNode(".//text()")?.InnerText.Trim() ?? "";

//        private int? ParseInt(string value) => int.TryParse(value, out var result) ? result : null;

//        private double? ParseDouble(string value)
//        {
//            value = value.Replace(',', '.');
//            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
//        }
//    }
//}


using Acornima;
using Acornima.Ast;
using HtmlAgilityPack;
using Jint;
using Newtonsoft.Json.Linq;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace PcBuilderApi.Scrapers.Implementation
{
    public class GpuScraper : IComponentScraper<Gpu>
    {
        private const string BaseUrl = "https://hotline.ua";

        public async Task<ScrapingResult<Gpu>> ScrapeAsync(string url, HttpClient client)
        {

            var html = await client.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var gpu = new Gpu();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();


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
                return new ScrapingResult<Gpu>(null, new List<Store>(), new List<ProductOffer>());
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


            var nuxtData = ExtractNuxtDataFromHtml(htmlDoc);
            if (nuxtData != null)
            {
                var resultSpecs = FindTokenByKey(nuxtData, "productValues");
                var edgesSpecs = resultSpecs?["edges"] as JArray;
                if (edgesSpecs != null)
                {
                    foreach (var edge in edgesSpecs)
                    {
                        var node = edge["node"];

                        var key = node?["title"]?.ToString().Trim();
                        var value = node?["value"]?.ToString().Trim();

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
                            case "productOnVendorSite":
                                {
                                    gpu.FactoryLink = node?["value"]?.ToString().Trim();
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    return new ScrapingResult<Gpu>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<Gpu>(null, new List<Store>(), new List<ProductOffer>());
            }



            // Отримуємо URL зображення
            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                gpu.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
            }


            var resultOffers = FindTokenByKey(nuxtData, "offers");
            var edgesOffers = resultOffers?["edges"] as JArray;
            if (edgesOffers != null)
            {
                foreach (var edge in edgesOffers)
                {
                    try
                    {
                        var node = edge["node"];
                        // Extract store information

                        var storeName = node?["firmTitle"]?.ToString().Trim();

                        var storeLogoUrl = node?["firmLogo"]?.ToString().Trim();
                        if (!string.IsNullOrEmpty(storeLogoUrl) && storeLogoUrl.StartsWith("/"))
                        {
                            storeLogoUrl = $"{BaseUrl}{storeLogoUrl}";
                        }



                        int likes = node?["reviewsPositiveNumber"]?.Value<int>() ?? 0;
                        int dislikes = node?["reviewsNegativeNumber"]?.Value<int>() ?? 0;


                        // Create or find store
                        var store = stores.FirstOrDefault(s => s.Name == storeName);
                        if (store == null)
                        {
                            store = new Store
                            {
                                Id = Guid.NewGuid(),
                                Name = storeName,
                                LogoUrl = storeLogoUrl,
                                Likes = likes,
                                Dislikes = dislikes
                            };
                            stores.Add(store);
                        }

                        decimal price = node?["price"]?.Value<decimal>() ?? 0;

                        var offerUrl = node?["conversionUrl"]?.ToString().Trim();
                        if (string.IsNullOrEmpty(offerUrl)) continue;

                        if (offerUrl.StartsWith("/"))
                            offerUrl = $"{BaseUrl}{offerUrl}";

                        // Create product offer
                        var offer = new ProductOffer
                        {
                            Id = Guid.NewGuid(),
                            Price = price,
                            ComponentType = SD.ComponentType.Gpu,
                            ComponentId = gpu.Id, // Will be set when saving to DB
                            ProductOfferUrl = offerUrl,
                            StoreId = store.Id
                        };

                        offers.Add(offer);
                    }
                    catch (Exception ex)
                    {
                        // Log exception if needed, but continue with other offers
                        Console.WriteLine($"Error scraping offer: {ex.Message}");
                    }
                }

            }
            gpu.Id = Guid.NewGuid();

            return new ScrapingResult<Gpu>(gpu, stores, offers);
        }


        public JObject? ExtractNuxtDataFromHtml(HtmlDocument doc)
        {

            // Шукаємо <script> з window.__NUXT__
            var scriptNode = doc.DocumentNode
                .SelectSingleNode("//script[contains(text(), 'window.__NUXT__')]");

            if (scriptNode == null)
                return null;

            string script = scriptNode.InnerHtml;

            // Шукаємо функцію (function(...) { ... })(...)
            var match = Regex.Match(script, @"window\.__NUXT__\s*=\s*(\(function[\s\S]*?\}\s*\(.*?\)\))");

            if (!match.Success)
                return null;

            string jsExpression = match.Groups[1].Value;

            try
            {
                var engine = new Engine();
                var result = engine.Evaluate(jsExpression).ToObject();


                // Перетворимо на JObject для зручності
                return JObject.FromObject(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при виконанні JS: {ex.Message}");
                return null;
            }
        }

        public JToken? FindTokenByKey(JToken? container, string key)
        {
            if (container == null)
                return null;

            if (container.Type == JTokenType.Object)
            {
                foreach (var prop in (JObject)container)
                {
                    if (prop.Key == key)
                        return prop.Value;

                    var found = FindTokenByKey(prop.Value, key);
                    if (found != null)
                        return found;
                }
            }
            else if (container.Type == JTokenType.Array)
            {
                foreach (var item in (JArray)container)
                {
                    var found = FindTokenByKey(item, key);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }


        private int? ParseInt(string value) => int.TryParse(value, out var result) ? result : null;

        private double? ParseDouble(string value)
        {
            value = value.Replace(',', '.');
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
        }
    }
}

