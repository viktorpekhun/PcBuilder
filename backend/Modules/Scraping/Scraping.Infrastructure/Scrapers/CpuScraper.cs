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
    public class CpuScraper : IComponentScraper<Cpu>
    {
        private const string BaseUrl = "https://hotline.ua";

        public async Task<ScrapingResult<Cpu>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<Cpu> componentsFromDb, ConcurrentBag<Store> storesFromDb, CancellationToken cancellationToken = default)
        {

            var html = await client.GetStringAsync(url, cancellationToken);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var cpu = new Cpu();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                cpu.Name = titleNode.InnerText.Trim();
                cpu.Name = Regex.Replace(cpu.Name, @"Процесор", "", RegexOptions.IgnoreCase).Trim();
                var match = Regex.Match(cpu.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                cpu.Name = Regex.Replace(cpu.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<Cpu>(null, new List<Store>(), new List<ProductOffer>());
            }

            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'description-text')]");
            if (descriptionNode != null)
            {
                cpu.Description.Uk = descriptionNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(modelInBrackets))
                {
                    cpu.Description.Uk = Regex.Replace(cpu.Description.Uk, $@"\({Regex.Escape(modelInBrackets)}\)", "");
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
                                cpu.Brand = value ?? string.Empty;
                                break;
                            case "Тип роз'єму":
                                cpu.Socket = value ?? string.Empty;
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
                                cpu.DimmType = value ?? string.Empty;
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
                            case "Максимальне тепловиділення TDP, Вт":
                                {
                                    cpu.Tdp = ParseInt(value);
                                    break;
                                }
                            case "Інтегрована графіка":
                                cpu.IntegratedGraphics = !value.Equals("немає", StringComparison.OrdinalIgnoreCase);
                                break;
                            case "Комплектація (Tray/Box/MPK)":
                                cpu.Complectation = value;
                                break;
                            case "productOnVendorSite":
                                {
                                    cpu.FactoryLink = node?["url"]?.ToString().Trim();
                                    break;
                                }
                        }
                    }
                    var existingCpu = componentsFromDb.FirstOrDefault(s => s.Name == cpu.Name);
                    if (existingCpu != null)
                    {
                        cpu.Id = existingCpu.Id;
                    }
                    else
                    {
                        cpu.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<Cpu>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<Cpu>(null, new List<Store>(), new List<ProductOffer>());
            }



            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                cpu.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = ComponentType.Cpu,
                            ComponentId = cpu.Id,
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
                cpu.AveragePrice = (decimal)avgPrice;
                cpu.OffersCount = offers.Count;
            }

            return new ScrapingResult<Cpu>(cpu, stores, offers);
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