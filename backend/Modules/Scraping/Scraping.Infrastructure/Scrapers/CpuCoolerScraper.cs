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
    public class CpuCoolerScraper : IComponentScraper<CpuCooler>
    {
        private const string BaseUrl = "https://hotline.ua";
        public async Task<ScrapingResult<CpuCooler>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<CpuCooler> componentsFromDb, ConcurrentBag<Store> storesFromDb, CancellationToken cancellationToken = default)
        {
            var html = await client.GetStringAsync(url, cancellationToken);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var cpuCooler = new CpuCooler { HotlineUrl = url };
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                cpuCooler.Name = titleNode.InnerText.Trim();
                cpuCooler.Name = Regex.Replace(cpuCooler.Name, @"Повітряне охолодження", "", RegexOptions.IgnoreCase).Trim();
                cpuCooler.Name = Regex.Replace(cpuCooler.Name, @"Водяне охолодження", "", RegexOptions.IgnoreCase).Trim();
                var match = Regex.Match(cpuCooler.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                cpuCooler.Name = Regex.Replace(cpuCooler.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<CpuCooler>(null, new List<Store>(), new List<ProductOffer>());
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
                                cpuCooler.Brand = value ?? string.Empty;
                                break;
                            case "Тип":
                                value = value?.ToLower();
                                if (value == "водяне охолодження")
                                {
                                    cpuCooler.Type = "Water";
                                }
                                else
                                {
                                    cpuCooler.Type = "Air";
                                }
                                break;
                            case "Кількість вентиляторів":
                                cpuCooler.FanCount = ParseInt(value);
                                break;
                            case "Розміри вентилятора, мм":
                                var match = Regex.Match(value, @"\d+");
                                if (match.Success)
                                {
                                    cpuCooler.FanSize = ParseDouble(match.Value);
                                }
                                else
                                {
                                    cpuCooler.FanSize = null;
                                }
                                break;
                            case "Матеріал радіатора":
                                {
                                    if (string.IsNullOrEmpty(value))
                                    {
                                        cpuCooler.RadiatorMaterial = null;
                                        break;
                                    }

                                    cpuCooler.RadiatorMaterial = new LocalizedString { Uk = char.ToUpper(value[0]) + value.Substring(1) };
                                    break;
                                }
                            case "Регулювання обертів":
                                cpuCooler.SpeedControl = value ?? string.Empty;
                                break;
                            case "Роз'єм":
                                cpuCooler.PowerConnector = value ?? string.Empty;
                                break;
                            case "Розсіювана потужність, Вт":
                                cpuCooler.MaxPowerDissipation = ParseInt(value);
                                break;
                            case "Мінімальна швидкість обертання, об / хв":
                                cpuCooler.MinSpeed = ParseInt(value);
                                break;
                            case "Максимальна швидкість обертання, об / хв":
                                cpuCooler.MaxSpeed = ParseInt(value);
                                break;
                            case "Повітряний потік (CFM)":
                                cpuCooler.AirflowCfm = ParseDouble(value);
                                break;
                            case "Рівень шуму, дБ":
                                if (!string.IsNullOrEmpty(value) && value.Contains('-'))
                                {
                                    var parts = value.Split('-');
                                    cpuCooler.NoiseLevelDb = ParseDouble(parts[1]);
                                }
                                else
                                {
                                    cpuCooler.NoiseLevelDb = ParseDouble(value);
                                }
                                break;
                            case "Напруга живлення, В":
                                cpuCooler.Voltage = ParseInt(value);
                                break;
                            case "Час безвідмовної роботи, г":
                                cpuCooler.Lifespan = ParseInt(value);
                                break;
                            case "Розміри кулера, мм":
                                var normalized = value?.Replace("х", "x").Replace("Х", "x").ToLower() ?? string.Empty;
                                var matches = Regex.Matches(normalized, @"[\d.,]+");

                                if (matches.Count >= 1)
                                    cpuCooler.Length = ParseDouble(matches[0].Value);
                                if (matches.Count >= 2)
                                    cpuCooler.Width = ParseDouble(matches[1].Value);
                                if (matches.Count >= 3)
                                    cpuCooler.Height = ParseDouble(matches[2].Value);
                                break;
                            case "Маса, г":
                                value = value?.Replace(" ", "");
                                cpuCooler.Weight = ParseDouble(value);
                                break;
                            case "productOnVendorSite":
                                cpuCooler.FactoryLink = node?["url"]?.ToString().Trim();
                                break;
                            case "Socket 1700, 1851" or "Socket 1200, 115x" or "Socket 2066, 2011":
                                if (value == "є")
                                {
                                    var sockets = key.Replace("Socket", "")
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim());
                                    foreach (var socket in sockets)
                                    {
                                        socketsList.Add(new CpuCoolerSocket
                                        {
                                            SocketType = "Socket " + socket
                                        });
                                    }
                                }
                                break;
                            case "Socket AM5" or "Socket AM4" or "Socket 3647" or "Socket 1366" or "Socket 775":
                                if (value == "є")
                                {
                                    socketsList.Add(new CpuCoolerSocket
                                    {
                                        SocketType = key
                                    });
                                }
                                break;
                            case "Socket AM2(+)/AM3(+)/FM1/FM2" or "Socket TR4 / SP3":
                                if (value == "є")
                                {
                                    var sockets = key.Replace("Socket", "")
                                        .Split('/', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim());
                                    foreach (var socket in sockets)
                                    {
                                        if (socket.Contains('+'))
                                        {
                                            var socketNew = socket.Replace("(+)", "");
                                            socketsList.Add(new CpuCoolerSocket
                                            {
                                                SocketType = "Socket " + socketNew
                                            });
                                            socketsList.Add(new CpuCoolerSocket
                                            {
                                                SocketType = "Socket " + socketNew + " +"
                                            });
                                        }
                                        else
                                        {
                                            socketsList.Add(new CpuCoolerSocket
                                            {
                                                SocketType = "Socket " + socket
                                            });
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    if (cpuCooler.Type == "Water")
                    {
                        cpuCooler.Wattage = 15;
                    }
                    else
                    {
                        cpuCooler.Wattage = 10;
                    }

                    cpuCooler.CpuCoolerSockets = socketsList;

                    var existingCpuCooler = componentsFromDb.FirstOrDefault(s => s.Name == cpuCooler.Name);
                    if (existingCpuCooler != null)
                    {
                        cpuCooler.Id = existingCpuCooler.Id;
                    }
                    else
                    {
                        cpuCooler.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<CpuCooler>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<CpuCooler>(null, new List<Store>(), new List<ProductOffer>());
            }



            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                cpuCooler.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = ComponentType.CpuCooler,
                            ComponentId = cpuCooler.Id,
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
                var avgPrice = offers.Any() ? Math.Round(offers.Average(p => p.Price)) : 0; cpuCooler.AveragePrice = (decimal)avgPrice;
                cpuCooler.OffersCount = offers.Count;
            }
            

            return new ScrapingResult<CpuCooler>(cpuCooler, stores, offers);
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
