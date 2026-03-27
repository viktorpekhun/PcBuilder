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
using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;

namespace Scraping.Infrastructure.Scrapers
{
    public class SsdScraper : IComponentScraper<Ssd>
    {
        private const string BaseUrl = "https://hotline.ua";
        public async Task<ScrapingResult<Ssd>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<Ssd> componentsFromDb, ConcurrentBag<Store> storesFromDb)
        {
            var html = await client.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var ssd = new Ssd();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                ssd.Name = titleNode.InnerText.Trim();
                var match = Regex.Match(ssd.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                ssd.Name = Regex.Replace(ssd.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<Ssd>(null, new List<Store>(), new List<ProductOffer>());
            }

            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description__content')]");
            if (descriptionNode != null)
            {
                ssd.Description = descriptionNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(modelInBrackets))
                {
                    ssd.Description = Regex.Replace(ssd.Description, $@"\({Regex.Escape(modelInBrackets)}\)", "");
                }
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
                                ssd.Brand = value ?? string.Empty;
                                break;
                            case "Обсяг, ГБ":
                                var capacityMatch = Regex.Match(value, @"\d+");
                                if (capacityMatch.Success)
                                    ssd.Capacity = int.Parse(capacityMatch.Value);
                                break;
                            case "Інтерфейс":
                                ssd.Interface = value ?? string.Empty;
                                break;
                            case "Тип флеш-пам'яті NAND":
                                ssd.NandType = value ?? string.Empty;
                                break;
                            case "Підтримка TRIM":
                                if (value?.ToLower() == "є")
                                    ssd.IsTrimmSupported = true;
                                else
                                    ssd.IsTrimmSupported = false;
                                break;
                            case "Форм-фактор":
                                ssd.FormFactor = value ?? string.Empty;
                                break;
                            case "Розміри, мм":
                                ssd.Size = value ?? string.Empty;
                                break;
                            case "Маса, г":
                                var weightMatch = Regex.Match(value, @"\d+([.,]\d+)?");
                                if (weightMatch.Success)
                                    ssd.Weight = ParseDouble(weightMatch.Value);
                                break;
                            case "Максимальна швидкість читання, МБ/с":
                                ssd.MaxReadSpeed = ParseInt(value);
                                break;
                            case "Максимальна швидкість запису, МБ/с":
                                ssd.MaxWriteSpeed = ParseInt(value);
                                break;
                            case "Швидкість випадкового читання блоками 4KB, IOPS":
                                ssd.RandomReadSpeed = ParseInt(value);
                                break;
                            case "Швидкість випадкового запису блоками 4KB, IOPS":
                                ssd.RandomWriteSpeed = ParseInt(value);
                                break;
                            case "Ресурс запису (TBW), TB":
                                ssd.WritingRecource = ParseInt(value);
                                break;
                            case "Середній час безвідмовної роботи (MTBF), млн. годин":
                                var avgTimeMatch = Regex.Match(value, @"\d+([.,]\d+)?");
                                if (avgTimeMatch.Success)
                                    ssd.AverageLifeTime = ParseDouble(avgTimeMatch.Value)*1000000.0;
                                break;
                            case "productOnVendorSite":
                                ssd.FactoryLink = node?["url"]?.ToString().Trim();
                                break;
                        }
                    }
                    
                    if (ssd.Capacity == 0)
                    {
                        return new ScrapingResult<Ssd>(null, new List<Store>(), new List<ProductOffer>());
                    }
                    ssd.Wattage = 10;
                    var existingSsd = componentsFromDb.FirstOrDefault(s => s.Name == ssd.Name);
                    if (existingSsd != null)
                    {
                        ssd.Id = existingSsd.Id;
                    }
                    else
                    {
                        ssd.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<Ssd>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<Ssd>(null, new List<Store>(), new List<ProductOffer>());
            }

            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                ssd.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = ComponentType.Ssd,
                            ComponentId = ssd.Id,
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
                ssd.AveragePrice = (decimal)avgPrice;
                ssd.OffersCount = offers.Count;
            }


            return new ScrapingResult<Ssd>(ssd, stores, offers);
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
