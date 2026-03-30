using Scraping.Application;
using Scraping.Application.Interfaces;
﻿
using HtmlAgilityPack;
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
    public class GpuScraper : IComponentScraper<Gpu>
    {
        private const string BaseUrl = "https://hotline.ua";

        public async Task<ScrapingResult<Gpu>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<Gpu> componentsFromDb, ConcurrentBag<Store> storesFromDb)
        {

            var html = await client.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var gpu = new Gpu();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                gpu.Name = titleNode.InnerText.Trim();
                gpu.Name = Regex.Replace(gpu.Name, @"Відеокарта", "", RegexOptions.IgnoreCase).Trim();
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

            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'description-text')]");
            if (descriptionNode != null)
            {
                gpu.Description.Uk = descriptionNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(modelInBrackets))
                {
                    gpu.Description.Uk = Regex.Replace(gpu.Description.Uk, $@"\({Regex.Escape(modelInBrackets)}\)", "");
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
                                gpu.Brand = value ?? string.Empty;
                                break;
                            case "Виробник GPU":
                                gpu.GpuManufacturer = value ?? string.Empty;
                                break;
                            case "GPU":
                                gpu.GpuModel = value ?? string.Empty;
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
                                var normalized = value?.Replace("х", "x").Replace("Х", "x").ToLower() ?? string.Empty;
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

                                    var parts = value?.Split('+', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                                    
                                    if (parts.Length == 0) break;

                                    foreach (var part in parts)
                                    {
                                        var match = Regex.Match(part.Trim(), @"(\d+)\s*x\s*(\d+)", RegexOptions.IgnoreCase);

                                        if (match.Success)
                                        {
                                            int quantity = int.Parse(match.Groups[1].Value);
                                            int pins = int.Parse(match.Groups[2].Value);

                                            connectors.Add(new GpuPowerConnector
                                            {
                                                Quantity = quantity,
                                                Pins = pins
                                            });
                                        }
                                    }

                                    gpu.GpuPowerConnectors = connectors;
                                    break;
                                }
                            case "productOnVendorSite":
                                {
                                    gpu.FactoryLink = node?["url"]?.ToString().Trim();
                                    break;
                                }
                        }
                    }
                    if (string.IsNullOrEmpty(gpu.GpuModel))
                    {
                        return new ScrapingResult<Gpu>(null, new List<Store>(), new List<ProductOffer>());
                    }
                    var existingGpu = componentsFromDb.FirstOrDefault(s => s.Name == gpu.Name);
                    if (existingGpu != null)
                    {
                        gpu.Id = existingGpu.Id;
                    }
                    else
                    {
                        gpu.Id = Guid.NewGuid();
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



            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                gpu.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = ComponentType.Gpu,
                            ComponentId = gpu.Id,
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
                gpu.AveragePrice = (decimal)avgPrice;
                gpu.OffersCount = offers.Count;
            }
            

            return new ScrapingResult<Gpu>(gpu, stores, offers);
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

