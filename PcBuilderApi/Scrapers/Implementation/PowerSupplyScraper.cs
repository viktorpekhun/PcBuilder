using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PcBuilderApi.Scrapers.Implementation
{
    public class PowerSupplyScraper : IComponentScraper<PowerSupply>
    {
        private const string BaseUrl = "https://hotline.ua";

        public async Task<ScrapingResult<PowerSupply>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<PowerSupply> componentsFromDb, ConcurrentBag<Store> storesFromDb)
        {

            var html = await client.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var powerSupply = new PowerSupply();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                powerSupply.Name = titleNode.InnerText.Trim();
                var match = Regex.Match(powerSupply.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                powerSupply.Name = Regex.Replace(powerSupply.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<PowerSupply>(null, new List<Store>(), new List<ProductOffer>());
            }

            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description__content')]");
            if (descriptionNode != null)
            {
                powerSupply.Description = descriptionNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(modelInBrackets))
                {
                    powerSupply.Description = Regex.Replace(powerSupply.Description, $@"\({Regex.Escape(modelInBrackets)}\)", "");
                }
            }


            var nuxtData = NuxtScriptWorker.ExtractNuxtDataFromHtml(htmlDoc);
            if (nuxtData != null)
            {
                var resultSpecs = NuxtScriptWorker.FindTokenByKey(nuxtData, "productValues");
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
                            case "vendor":
                                powerSupply.Brand = value ?? string.Empty;
                                break;
                            case "Форм-фактор БЖ":
                                powerSupply.FormFactor = value ?? string.Empty;
                                break;
                            case "Потужність сумарна, Вт":
                                var wattageMatch = Regex.Match(value, @"\d+");
                                if (wattageMatch.Success)
                                    powerSupply.Wattage = int.Parse(wattageMatch.Value);
                                break;
                            case "Molex":
                                var molexMatch = Regex.Match(value, @"\d+");
                                if (molexMatch.Success)
                                    powerSupply.MolexCount = ParseInt(molexMatch.Value);
                                else
                                    powerSupply.MolexCount = null;
                                break;
                            case "SATA":
                                var sataMatch = Regex.Match(value, @"\d+");
                                if (sataMatch.Success)
                                    powerSupply.SataCount = ParseInt(sataMatch.Value);
                                else
                                    powerSupply.SataCount = null;
                                break;
                            case "FDD":
                                var fddMatch = Regex.Match(value, @"\d+");
                                if (fddMatch.Success)
                                    powerSupply.FddCount = ParseInt(fddMatch.Value);
                                else
                                    powerSupply.FddCount = null;
                                break;
                            case "Вхідна напруга, В (діапазон)":
                                var voltageMatch = Regex.Match(value, @"^(?<min>\d+)(?:\s*-\s*(?<max>\d+))?\s*\D*$");
                                if (voltageMatch.Success)
                                {

                                    if (voltageMatch.Groups["max"].Success)
                                    {
                                        powerSupply.InputMinVoltage = ParseInt(voltageMatch.Groups["min"].Value);
                                        powerSupply.InputMaxVoltage = ParseInt(voltageMatch.Groups["max"].Value);
                                    }
                                    else
                                    {
                                        powerSupply.InputMaxVoltage = ParseInt(voltageMatch.Groups["min"].Value);
                                    }
                                }
                                break;
                            case "Функція APFC (Active Power Factor Correction)":
                                if (value?.ToLower() == "є")
                                {
                                    powerSupply.HasApcf = true;
                                }
                                else
                                {
                                    powerSupply.HasApcf = false;
                                }
                                break;
                            case "Стандарт 80 PLUS":
                                powerSupply.EfficiencyStandart = value ?? string.Empty;
                                break;
                            case "ККД (%)":
                                powerSupply.EfficiencyPercent = ParseDouble(value);
                                break;
                            case "Модульне підключення кабелів":
                                if (value?.ToLower() == "є" || value.Contains('+'))
                                {
                                    powerSupply.IsModular = true;
                                }
                                else
                                {
                                    powerSupply.IsModular = false;
                                }
                                break;
                            case "Рівень шуму, дБ":
                                if (!string.IsNullOrEmpty(value) && value.Contains('-'))
                                {
                                    var parts = value.Split('-');
                                    powerSupply.NoiseLevelMaxDb = ParseDouble(parts[1]);
                                }
                                else
                                {
                                    powerSupply.NoiseLevelMaxDb = ParseDouble(value);
                                }
                                break;
                            case "Розміри, мм":
                                powerSupply.Size = value ?? string.Empty;
                                break;
                            case "productOnVendorSite":
                                {
                                    powerSupply.FactoryLink = node?["url"]?.ToString().Trim();
                                    break;
                                }
                            case "Тип роз'єму підключення до материнської плати":
                                {
                                    var mbPinsMatch = Regex.Match(value, @"^(?<main>\d+)(?:\s*\+\s*(?<extra>\d+))?\s*[a-zA-Z]*$");

                                    if (mbPinsMatch.Success)
                                    {
                                        int pins = int.Parse(mbPinsMatch.Groups["main"].Value);
                                        int? additionalPins = null;
                                        if (mbPinsMatch.Groups["extra"].Success)
                                        {
                                            additionalPins = ParseInt(mbPinsMatch.Groups["extra"].Value);
                                        }
                                        powerSupply.PowerSupplyPowerConnectors.Add(new PowerSupplyPowerConnector
                                        {
                                            Type = "Motherboard",
                                            Pins = pins,
                                            AdditionalPins = additionalPins,
                                            Quantity = 1,
                                        });
                                    }
                                    break;
                                }
                            case "Тип роз'єму підключення живлення процесора":
                                {
                                    var connectors = new List<PowerSupplyPowerConnector>();
                                    var cpuMatch = Regex.Match(value, @"\(([^)]+)\)|^([^\(\)]+)$");
                                    var cpuMatchResult = cpuMatch.Groups[1].Success ? cpuMatch.Groups[1].Value : cpuMatch.Groups[2].Value;
                                    if (cpuMatch.Success)
                                    {
                                        var pinsList = cpuMatchResult
                                            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim());

                                        var regex = new Regex(@"(?<qty>\d+)\s*[xх]\s*(?<pins>\d+)\s*(?:\+\s*(?<addpins>\d+))?", RegexOptions.IgnoreCase);

                                        foreach (var val in pinsList)
                                        {
                                            var match = regex.Match(val);

                                            if (!match.Success) continue;

                                            int quantity = int.Parse(match.Groups["qty"].Value);
                                            int pins = int.Parse(match.Groups["pins"].Value);
                                            int? additionalPins = match.Groups["addpins"].Success ? int.Parse(match.Groups["addpins"].Value) : null;

                                            connectors.Add(new PowerSupplyPowerConnector
                                            {
                                                Type = "CPU",
                                                Pins = pins,
                                                AdditionalPins = additionalPins,
                                                Quantity = quantity,
                                            });
                                        }
                                        powerSupply.PowerSupplyPowerConnectors.AddRange(connectors);
                                    }
                                    break;
                                }
                            case "Роз'єми дод. живлення для відеокарт":
                                {
                                    var connectors = new List<PowerSupplyPowerConnector>();
                                    if (value.ToLower().Contains("16"))
                                    {
                                        value = value.ToLower().Replace("16", "12+4");
                                        if (value.ToLower().Contains("(12vhpwr)"))
                                        {
                                            value = value.ToLower().Replace("(12vhpwr)", "");
                                        }
                                    }
                                    if (value.ToLower().Contains("(12vhpwr)"))
                                    {
                                        value = value.ToLower().Replace("(12vhpwr)", "1x12+4pin");
                                    }
                                    if (value.ToLower().Contains("12vhpwr"))
                                    {
                                        value = value.ToLower().Replace("12vhpwr", "12+4pin");
                                    }
                                    var gpuMatch = Regex.Match(value, @"\(([^)]+)\)|^([^\(\)]+)$");
                                    var gpuMatchResult = gpuMatch.Groups[1].Success ? gpuMatch.Groups[1].Value : gpuMatch.Groups[2].Value;
                                    if (gpuMatch.Success)
                                    {
                                        var pinsList = gpuMatchResult
                                            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim());

                                        var regex = new Regex(@"(?<qty>\d+)\s*[xх]\s*(?<pins>\d+)\s*(?:\+\s*(?<addpins>\d+))?", RegexOptions.IgnoreCase);

                                        foreach (var val in pinsList)
                                        {
                                            var match = regex.Match(val);

                                            if (!match.Success) continue;

                                            int quantity = int.Parse(match.Groups["qty"].Value);
                                            int pins = int.Parse(match.Groups["pins"].Value);
                                            int? additionalPins = match.Groups["addpins"].Success ? int.Parse(match.Groups["addpins"].Value) : null;

                                            connectors.Add(new PowerSupplyPowerConnector
                                            {
                                                Type = "GPU",
                                                Pins = pins,
                                                AdditionalPins = additionalPins,
                                                Quantity = quantity,
                                            });
                                        }
                                        powerSupply.PowerSupplyPowerConnectors.AddRange(connectors);
                                    }
                                    break;
                                }

                        }
                    }
                    var existingPowerSupply = componentsFromDb.FirstOrDefault(s => s.Name == powerSupply.Name);
                    if (existingPowerSupply != null)
                    {
                        powerSupply.Id = existingPowerSupply.Id;
                    }
                    else
                    {
                        powerSupply.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<PowerSupply>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<PowerSupply>(null, new List<Store>(), new List<ProductOffer>());
            }


            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                powerSupply.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
            }

            var resultOffers = NuxtScriptWorker.FindTokenByKey(nuxtData, "offers");
            var edgesOffers = resultOffers?["edges"] as JArray;
            if (edgesOffers != null)
            {
                foreach (var edge in edgesOffers)
                {
                    try
                    {
                        var node = edge["node"];
                        if (node == null) continue;

                        var storeName = node?["firmTitle"]?.ToString().Trim();
                        if (string.IsNullOrEmpty(storeName)) continue;

                        var storeLogoUrl = node?["firmLogo"]?.ToString().Trim();
                        if (!string.IsNullOrEmpty(storeLogoUrl) && storeLogoUrl.StartsWith("/"))
                        {
                            storeLogoUrl = $"{BaseUrl}{storeLogoUrl}";
                        }

                        int likes = node?["reviewsPositiveNumber"]?.Value<int?>() ?? 0;
                        int dislikes = node?["reviewsNegativeNumber"]?.Value<int?>() ?? 0;

                        var store = stores.FirstOrDefault(s => s.Name == storeName);
                        var storeFromDb = storesFromDb.FirstOrDefault(s => s.Name == storeName);
                        if (store == null)
                        {

                            store = new Store
                            {
                                Name = storeName,
                                LogoUrl = storeLogoUrl,
                                Likes = likes,
                                Dislikes = dislikes
                            };
                            if (storeFromDb != null)
                            {
                                store.Id = storeFromDb.Id;
                            }
                            else
                            {
                                store.Id = Guid.NewGuid();
                            }
                            stores.Add(store);
                        }

                        decimal price = node?["price"]?.Value<decimal>() ?? 0;

                        var offerUrl = node?["conversionUrl"]?.ToString().Trim();
                        if (string.IsNullOrEmpty(offerUrl)) continue;


                        if (!string.IsNullOrEmpty(offerUrl) && offerUrl.StartsWith("/"))
                        {
                            offerUrl = $"{BaseUrl}{offerUrl}";
                        }

                        var offer = new ProductOffer
                        {
                            Id = Guid.NewGuid(),
                            Price = price,
                            ComponentType = SD.ComponentType.PowerSupply,
                            ComponentId = powerSupply.Id,
                            ProductOfferUrl = offerUrl,
                            StoreId = store.Id
                        };

                        offers.Add(offer);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error scraping offer: {ex.Message}");
                    }
                }
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;
                powerSupply.AveragePrice = (decimal)avgPrice;
                powerSupply.OffersCount = offers.Count;
            }

            return new ScrapingResult<PowerSupply>(powerSupply, stores, offers);
        }

        private int? ParseInt(string? value) => int.TryParse(value, out var result) ? result : null;

        private double? ParseDouble(string? value)
        {
            if (value == null) return null;
            value = value.Replace(',', '.');
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
        }
    }
}
