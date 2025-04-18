using HtmlAgilityPack;
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
                return new ScrapingResult<Gpu>(null, new List<Store>(), new List<ProductOffer>());
            }

            // Отримуємо URL зображення
            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                gpu.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
            }

            var offersContainer = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'list-container')]");
            if (offersContainer != null)
            {
                var lastNode = offersContainer.SelectNodes("./div").LastOrDefault();
                if (lastNode != null)
                {
                    foreach (var offerNode in lastNode.SelectNodes("./div"))
                    {
                        try
                        {
                            // Extract store information
                            var storeNode = offerNode.SelectSingleNode(".//div[contains(@class, 'shop__header')]");
                            if (storeNode == null) continue;
                            var storeName = storeNode.InnerText.Trim();

                            var storeALogoNode = offerNode.SelectSingleNode(".//a[contains(@class, 'shop__logo')]");

                            var storeLogoNode = storeALogoNode.SelectSingleNode(".//img");
                            var storeLogoUrl = storeLogoNode?.GetAttributeValue("src", null);
                            if (!string.IsNullOrEmpty(storeLogoUrl) && storeLogoUrl.StartsWith("/"))
                            {
                                storeLogoUrl = $"{BaseUrl}{storeLogoUrl}";
                            }

                            // Extract store ratings
                            var ratingNode = offerNode.SelectSingleNode(".//div[contains(@class, 'shop__rating')]");
                            int likes = 0, dislikes = 0;
                            if (ratingNode != null)
                            {
                                var likesNode = ratingNode.SelectSingleNode(".//span[contains(@class, 'shop__rating-icon--like')]");
                                var dislikesNode = ratingNode.SelectSingleNode(".//span[contains(@class, 'shop__rating-icon--dislike')]");

                                if (likesNode != null)
                                {
                                    var likesText = likesNode.InnerText.Trim();
                                    int.TryParse(Regex.Match(likesText, @"\d+").Value, out likes);
                                }

                                if (dislikesNode != null)
                                {
                                    var dislikesText = dislikesNode.InnerText.Trim();
                                    int.TryParse(Regex.Match(dislikesText, @"\d+").Value, out dislikes);
                                }
                            }

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

                            // Extract price
                            var priceNode = offerNode.SelectSingleNode(".//span[not(contains(@style, 'display: none')) and contains(@data-v-3777e10c, '') and contains(@data-v-0095f7a0, '')]");
                            if (priceNode == null) continue;
                            var text = HtmlEntity.DeEntitize(priceNode.InnerText);

                            var priceValue = text.Replace("грн", "").Replace("\u00A0", "").Replace(" ", "").Trim();
                            if (!decimal.TryParse(priceValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                                continue;

                            // Extract offer URL
                            var offerLinkNode = offerNode.SelectSingleNode(".//a[contains(@href, '/go/price')]");
                            if (offerLinkNode == null) continue;

                            var offerUrl = offerLinkNode.GetAttributeValue("href", "");
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
            }
            gpu.Id = Guid.NewGuid();

            return new ScrapingResult<Gpu>(gpu, stores, offers);
        }

        private string ExtractText(HtmlNode node)
        {
            return HtmlEntity.DeEntitize(node?.InnerText ?? "").Trim();
        }

        private int? ParseInt(string value) => int.TryParse(value, out var result) ? result : null;

        private double? ParseDouble(string value)
        {
            value = value.Replace(',', '.');
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
        }
    }
}

