using Scraping.Application;
using Scraping.Application.Interfaces;
﻿using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Components.Domain.Entities;
using PcBuilder.SharedKernel;
using Scraping.Infrastructure.Utilities;
using PcBuilder.SharedKernel.Enums;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Scraping.Infrastructure.Scrapers
{
    public class RamScraper : IComponentScraper<Ram>
    {
        private const string BaseUrl = "https://hotline.ua";
        public async Task<ScrapingResult<Ram>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<Ram> componentsFromDb, ConcurrentBag<Store> storesFromDb, CancellationToken cancellationToken = default)
        {
            var html = await client.GetStringAsync(url, cancellationToken);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var ram = new Ram();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                ram.Name = titleNode.InnerText.Trim();
                ram.Name = Regex.Replace(ram.Name, @"Пам'ять для настільних комп'ютерів", "", RegexOptions.IgnoreCase).Trim();
                var match = Regex.Match(ram.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                ram.Name = Regex.Replace(ram.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<Ram>(null, new List<Store>(), new List<ProductOffer>());
            }


            var nuxtData = NuxtScriptWorker.ExtractNuxtDataFromHtml(htmlDoc);
            if (nuxtData != null)
            {
                var resultSpecs = NuxtScriptWorker.FindTokenByKey(nuxtData, "productValues");
                var edgesSpecs = resultSpecs?["edges"] as JArray;
                if (edgesSpecs != null)
                {
                    var socketsList = new List<CpuCoolerSocket>();
                    foreach (var edge in edgesSpecs)
                    {
                        var node = edge["node"];

                        var key = node?["title"]?.ToString().Trim();
                        var value = node?["value"]?.ToString().Trim();

                        switch (key)
                        {
                            case "vendor":
                                ram.Brand = value ?? string.Empty;
                                break;
                            case "Тип":
                                ram.Type = value ?? string.Empty;
                                break;
                            case "Ефективна частота, МГц":
                                {
                                    var frequencyMatch = Regex.Match(value, @"\d+");
                                    if (frequencyMatch.Success)
                                        ram.Frequency = int.Parse(frequencyMatch.Value);
                                    else
                                        ram.Frequency = 0;
                                    break;
                                }
                            case "Ефективна частота, МТ/с":
                                {
                                    var frequencyMatch = Regex.Match(value, @"\d+");
                                    if (frequencyMatch.Success)
                                        ram.Frequency = int.Parse(frequencyMatch.Value);
                                    else
                                        ram.Frequency = 0;
                                    break;
                                }
                            case "Обсяг, ГБ":
                                var capacityMatch = Regex.Match(value, @"\d+");
                                if (capacityMatch.Success)
                                    ram.Capacity = ParseInt(capacityMatch.Value);
                                break;
                            case "Кількість планок в комплекті":
                                var moduleMatch = Regex.Match(value, @"\d+");
                                if (moduleMatch.Success)
                                    ram.ModuleQuantity = ParseInt(moduleMatch.Value);
                                break;
                            case "Штатні таймінги":
                                ram.Timings = value ?? string.Empty;
                                break;
                            case "Робоча напруга, В":
                                ram.Voltage = ParseDouble(value);
                                break;
                            case "Підтримка XMP":
                                if (value == "є" || value.Contains('+'))
                                    ram.Xmp = true;
                                else
                                    ram.Xmp = false;
                                break;
                            case "Перевірка і корекція помилок (ECC)":
                                if (value.ToLower() == "є" || value.ToLower() == "ecc")
                                    ram.Ecc = true;
                                else
                                    ram.Ecc = false;
                                break;
                            case "Підтримка AMD EXPO":
                                if (value?.ToLower() == "є")
                                    ram.Expo = true;
                                else
                                    ram.Expo = false;
                                break;
                            case "Буферизація":
                                ram.Bufferization = value ?? string.Empty;
                                break;
                            case "Колір":
                                ram.Color = value ?? string.Empty;
                                break;
                            case "productOnVendorSite":
                                ram.FactoryLink = node?["url"]?.ToString().Trim();
                                break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(ram.Type) || ram.Frequency == 0 || ram.Capacity == null || ram.ModuleQuantity == null)
                    {
                        return new ScrapingResult<Ram>(null, new List<Store>(), new List<ProductOffer>());
                    }

                    ram.Capacity = ram.Capacity / (ram.ModuleQuantity);
                    ram.Wattage = EstimateRamPower(ram.ModuleQuantity.Value, ram.Type, ram.Capacity.Value);

                    var existingRam = componentsFromDb.FirstOrDefault(s => s.Name == ram.Name);
                    if (existingRam != null)
                    {
                        ram.Id = existingRam.Id;
                    }
                    else
                    {
                        ram.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<Ram>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<Ram>(null, new List<Store>(), new List<ProductOffer>());
            }



            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                ram.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = ComponentType.Ram,
                            ComponentId = ram.Id,
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
                ram.AveragePrice = (decimal)avgPrice;
                ram.OffersCount = offers.Count;
            }


            return new ScrapingResult<Ram>(ram, stores, offers);
        }
        private int? ParseInt(string? value) => int.TryParse(value, out var result) ? result : null;

        private double? ParseDouble(string? value)
        {
            if (value == null) return null;
            value = value.Replace(',', '.');
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
        }

        private int EstimateRamPower(int count, string type, int capacityGb)
        {
            int basePowerPerModule = type.ToUpper() switch
            {
                "DDR2" => 2,
                "DDR3" => 2,
                "DDR4" => 3,
                "DDR5" => 4,
                _ => 3
            };

            int extraPowerPerModule = capacityGb switch
            {
                <= 8 => 0,
                <= 16 => 1,
                <= 32 => 2,
                <= 64 => 3,
                > 64 => 4
            };

            int perModulePower = basePowerPerModule + extraPowerPerModule;

            double total = count * perModulePower * 1.25;

            return (int)Math.Ceiling(total);
        }
    }
}
