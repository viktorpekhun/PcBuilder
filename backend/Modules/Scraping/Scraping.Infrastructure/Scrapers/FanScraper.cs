using Components.Domain.Entities;
using Components.Domain.ValueObjects;
﻿using HtmlAgilityPack;
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
    public class FanScraper : IComponentScraper<Fan>
    {
        private const string BaseUrl = "https://hotline.ua";
        public async Task<ScrapingResult<Fan>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<Fan> componentsFromDb, ConcurrentBag<Store> storesFromDb, CancellationToken cancellationToken = default)
        {
            var html = await client.GetStringAsync(url, cancellationToken);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var fan = new Fan();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                fan.Name = titleNode.InnerText.Trim();
                fan.Name = Regex.Replace(fan.Name, @"Вентилятор", "", RegexOptions.IgnoreCase).Trim();
                var match = Regex.Match(fan.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                fan.Name = Regex.Replace(fan.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<Fan>(null, new List<Store>(), new List<ProductOffer>());
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
                                fan.Brand = value ?? string.Empty;
                                break;
                            case "Кількість вентиляторів":
                                fan.ModuleCount = ParseInt(value);
                                break;
                            case "Тип підшипника":
                                {
                                    if (string.IsNullOrEmpty(value))
                                    {
                                        fan.BearingType = null;
                                        break;
                                    }

                                    fan.BearingType = new LocalizedString { Uk = char.ToUpper(value[0]) + value.Substring(1) };
                                    break;
                                }
                            case "Регулювання обертів":
                                fan.SpeedControl = value ?? string.Empty;
                                break;
                            case "Роз'єм":
                                fan.Connector = value ?? string.Empty;
                                break;
                            case "Колір вентилятора":
                                {
                                    if (string.IsNullOrEmpty(value))
                                    {
                                        fan.Color = null;
                                        break;
                                    }

                                    fan.Color = new LocalizedString { Uk = char.ToUpper(value[0]) + value.Substring(1) };
                                    break;
                                }
                            case "Мінімальна швидкість обертання, об / хв":
                                fan.MinSpeed = ParseInt(value);
                                break;
                            case "Максимальна швидкість обертання, об / хв":
                                fan.MaxSpeed = ParseInt(value);
                                break;
                            case "Повітряний потік (CFM)":
                                fan.AirflowCfm = ParseDouble(value);
                                break;
                            case "Рівень шуму, дБ":
                                if (!string.IsNullOrEmpty(value) && value.Contains('-'))
                                {
                                    var parts = value.Split('-');
                                    fan.NoiseLevelDb = ParseDouble(parts[1]);
                                }
                                else
                                {
                                    fan.NoiseLevelDb = ParseDouble(value);
                                }
                                break;
                            case "Напруга живлення, В":
                                fan.Voltage = ParseInt(value);
                                break;
                            case "Розміри вентилятора, мм":
                                var normalized = value?.Replace("х", "x").Replace("Х", "x").ToLower() ?? string.Empty;
                                var sizeMatches = Regex.Matches(normalized, @"[\d.,]+");

                                if (sizeMatches.Count >= 1)
                                    fan.SizeLength = ParseDouble(sizeMatches[0].Value);
                                if (sizeMatches.Count >= 2)
                                    fan.SizeWidth = ParseDouble(sizeMatches[1].Value);
                                if (sizeMatches.Count >= 3)
                                    fan.SizeHeight = ParseDouble(sizeMatches[2].Value);
                                break;
                            case "Маса, г":
                                value = value?.Replace(" ", "");
                                fan.Weight = ParseDouble(value);
                                break;                      
                            case "productOnVendorSite":
                                fan.FactoryLink = node?["url"]?.ToString().Trim();
                                break;
                        }
                    }
                    if (fan.ModuleCount == null)
                    {
                        return new ScrapingResult<Fan>(null, new List<Store>(), new List<ProductOffer>());
                    }

                    fan.Wattage = fan.ModuleCount * 5;

                    var existingFan = componentsFromDb.FirstOrDefault(s => s.Name == fan.Name);
                    if (existingFan != null)
                    {
                        fan.Id = existingFan.Id;
                    }
                    else
                    {
                        fan.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<Fan>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<Fan>(null, new List<Store>(), new List<ProductOffer>());
            }

            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                fan.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = ComponentType.Fan,
                            ComponentId = fan.Id,
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
                fan.AveragePrice = (decimal)avgPrice;
                fan.OffersCount = offers.Count;
            }

            return new ScrapingResult<Fan>(fan, stores, offers);
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
