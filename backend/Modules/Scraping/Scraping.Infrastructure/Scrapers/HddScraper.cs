using Components.Domain.Entities;
﻿using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application;
using Scraping.Application.Interfaces;
using Scraping.Infrastructure.Utilities;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Scraping.Infrastructure.Scrapers
{
    public class HddScraper : IComponentScraper<Hdd>
    {
        private const string BaseUrl = "https://hotline.ua";
        public async Task<ScrapingResult<Hdd>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<Hdd> componentsFromDb, ConcurrentBag<Store> storesFromDb, CancellationToken cancellationToken = default)
        {
            var html = await client.GetStringAsync(url, cancellationToken);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var hdd = new Hdd();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                hdd.Name = titleNode.InnerText.Trim();
                hdd.Name = Regex.Replace(hdd.Name, @"Жорсткий диск", "", RegexOptions.IgnoreCase).Trim();
                var match = Regex.Match(hdd.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                hdd.Name = Regex.Replace(hdd.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<Hdd>(null, new List<Store>(), new List<ProductOffer>());
            }

            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'description-text')]");
            if (descriptionNode != null)
            {
                hdd.Description.Uk = descriptionNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(modelInBrackets))
                {
                    hdd.Description.Uk = Regex.Replace(hdd.Description.Uk, $@"\({Regex.Escape(modelInBrackets)}\)", "");
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
                                hdd.Brand = value ?? string.Empty;
                                break;
                            case "Обсяг, ГБ":
                                {
                                    var capacityMatch = Regex.Match(value, @"\d+");
                                    if (capacityMatch.Success)
                                        hdd.Capacity = int.Parse(capacityMatch.Value);
                                    break;
                                }
                            case "Об'єм, ГБ":
                                {
                                    var capacityMatch = Regex.Match(value, @"\d+");
                                    if (capacityMatch.Success)
                                        hdd.Capacity = int.Parse(capacityMatch.Value);
                                    break;
                                }
                            case "Обсяг, ТБ":
                                {
                                    hdd.Capacity = ParseCapacityToGb(value);
                                    break;
                                }
                            case "Об'єм, ТБ":
                                {
                                    hdd.Capacity = ParseCapacityToGb(value);
                                    break;
                                }
                            case "Інтерфейс підключення":
                                hdd.Interface = value ?? string.Empty;
                                break;
                            case "Форм-фактор, дюйм":
                                hdd.FormFactor = value ?? string.Empty;
                                break;
                            case "Швидкість обертання шпинделя, об/хв":
                                hdd.SpindleSpeed = ParseInt(value);
                                break;
                            case "Буфер, МБ":
                                hdd.Cache = ParseInt(value);
                                break;
                            case "Зовнішня швидкість передачі даних (макс), Мбайт / сек (буфер - Host)":
                                hdd.Speed = ParseInt(value);
                                break;
                            case "Технологія запису":
                                hdd.WritingTechnology = value ?? string.Empty;
                                break;
                            case "Рівень шуму при читанні/записі, дБ":
                                hdd.NoiceDb = ParseInt(value);
                                break;
                            case "productOnVendorSite":
                                hdd.FactoryLink = node?["url"]?.ToString().Trim();
                                break;
                        }
                    }

                    if (hdd.Capacity == 0)
                    {
                        return new ScrapingResult<Hdd>(null, new List<Store>(), new List<ProductOffer>());
                    }

                    if (hdd.SpindleSpeed != null)
                    {
                        if (hdd.SpindleSpeed >= 7200)
                        {
                            hdd.Wattage = 20;
                        }
                        else
                        {
                            hdd.Wattage = 15;
                        }
                    }

                    var existingHdd = componentsFromDb.FirstOrDefault(s => s.Name == hdd.Name);
                    if (existingHdd != null)
                    {
                        hdd.Id = existingHdd.Id;
                    }
                    else
                    {
                        hdd.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<Hdd>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<Hdd>(null, new List<Store>(), new List<ProductOffer>());
            }

            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                hdd.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = ComponentType.Hdd,
                            ComponentId = hdd.Id,
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
                hdd.AveragePrice = (decimal)avgPrice;
                hdd.OffersCount = offers.Count;
            }


            return new ScrapingResult<Hdd>(hdd, stores, offers);
        }
        private int? ParseInt(string? value) => int.TryParse(value, out var result) ? result : null;
        public int ParseCapacityToGb(string input)
        {
            var match = Regex.Match(input, @"\d+([,\.]\d+)?");
            if (!match.Success) return 0;
            string normalizedValue = match.Value.Replace(',', '.');
            if (double.TryParse(normalizedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
            {
                return (int)Math.Round(value * 1000.0);
            }
            return 0;
        }

    }
}
