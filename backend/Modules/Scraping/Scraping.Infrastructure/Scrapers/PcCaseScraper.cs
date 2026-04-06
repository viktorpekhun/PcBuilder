using Scraping.Application;
using Scraping.Application.Interfaces;
﻿using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Components.Domain.Entities;
using Components.Domain.ValueObjects;
using PcBuilder.SharedKernel;
using Scraping.Infrastructure.Utilities;
using PcBuilder.SharedKernel.Enums;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Scraping.Infrastructure.Scrapers
{
    public class PcCaseScraper : IComponentScraper<PcCase>
    {
        private const string BaseUrl = "https://hotline.ua";

        public async Task<ScrapingResult<PcCase>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<PcCase> componentsFromDb, ConcurrentBag<Store> storesFromDb, CancellationToken cancellationToken = default)
        {

            var html = await client.GetStringAsync(url, cancellationToken);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var pcCase = new PcCase();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                pcCase.Name = titleNode.InnerText.Trim();
                pcCase.Name = Regex.Replace(pcCase.Name, @"Корпус", "", RegexOptions.IgnoreCase).Trim();
                var match = Regex.Match(pcCase.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                pcCase.Name = Regex.Replace(pcCase.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<PcCase>(null, new List<Store>(), new List<ProductOffer>());
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
                                pcCase.Brand = value ?? string.Empty;
                                break;
                            case "Типорозмір":
                                pcCase.SizeStandard = value ?? string.Empty;
                                break;
                            case "Габарити, мм":
                                pcCase.SizeDimentions = value ?? string.Empty;
                                break;
                            case "Маса, кг":
                                pcCase.Weight = ParseDouble(value);
                                break;
                            case "Потужність БЖ, Вт":
                                pcCase.PsuWattage = ParseInt(value);
                                break;
                            case "Розташування":
                                {
                                    if (string.IsNullOrEmpty(value))
                                    {
                                        pcCase.PsuLocation = null;
                                        break;
                                    }
                                    
                                    pcCase.PsuLocation = new LocalizedString { Uk = char.ToUpper(value[0]) + value.Substring(1) };
                                    break;
                                }
                            case "Максимальна висота процесорного кулера, мм":
                                var coolerLengthMatch = Regex.Match(value, @"\d+(\.\d+)?");
                                if (coolerLengthMatch.Success)
                                {
                                    pcCase.MaxCpuCoolerHeight = ParseDouble(coolerLengthMatch.Value);
                                }
                                else
                                {
                                    pcCase.MaxCpuCoolerHeight = null;
                                }
                                break;
                            case "Максимальна довжина відеокарти, мм":
                                var gpuLengthMatch = Regex.Match(value, @"\d+(\.\d+)?");
                                if (gpuLengthMatch.Success)
                                {
                                    pcCase.MaxGpuLength = ParseDouble(gpuLengthMatch.Value);
                                }
                                else
                                {
                                    pcCase.MaxGpuLength = null;
                                }
                                break;
                            case "пилові фільтри":
                                if (value?.ToLower() == "є")
                                {
                                    pcCase.HasDustFilters = true;
                                }
                                else
                                {
                                    pcCase.HasDustFilters = false;
                                }
                                break;
                            case "Вбудовані вентилятори/розташування":
                                {
                                    if (string.IsNullOrEmpty(value))
                                    {
                                        pcCase.BuiltInFans = null;
                                        break;
                                    }

                                    pcCase.BuiltInFans = new LocalizedString { Uk = char.ToUpper(value[0]) + value.Substring(1) };
                                    break;
                                }
                            case "Кількість 2,5 відсіків":
                                var slots25match = Regex.Match(value, @"\d+");
                                if (slots25match.Success)
                                    pcCase.Slot25Quant = ParseInt(slots25match.Value);
                                else
                                    pcCase.Slot25Quant = null;
                                break;
                            case "Кількість 3,5 відсіків внутрішніх":
                                var slots35match = Regex.Match(value, @"\d+");
                                if (slots35match.Success)
                                    pcCase.Slot35Quant = ParseInt(slots35match.Value);
                                else
                                    pcCase.Slot35Quant = null;
                                break;
                            case "Кількість 5,25 відсіків":
                                var slots525match = Regex.Match(value, @"\d+");
                                if (slots525match.Success)
                                    pcCase.Slot525Quant = ParseInt(slots525match.Value);
                                else
                                    pcCase.Slot525Quant = null;
                                break;
                            case "Кількість слотів розширення":
                                var expansionSlotsMatch = Regex.Match(value, @"\d+");
                                if (expansionSlotsMatch.Success)
                                    pcCase.ExpansionSlotQuant = ParseInt(expansionSlotsMatch.Value);
                                else
                                    pcCase.ExpansionSlotQuant = null;
                                break;
                            case "USB":
                                pcCase.Usb = value ?? string.Empty;
                                break;
                            case "Вихід на навушники":
                                if (value?.ToLower() == "є")
                                {
                                    pcCase.HasHeadphones = true;
                                }
                                else
                                {
                                    pcCase.HasHeadphones = false;
                                }
                                break;
                            case "Вхід для мікрофона":
                                if (value?.ToLower() == "є")
                                {
                                    pcCase.HasMicrophone = true;
                                }
                                else
                                {
                                    pcCase.HasMicrophone = false;
                                }
                                break;
                            case "productOnVendorSite":
                                {
                                    pcCase.FactoryLink = node?["url"]?.ToString().Trim();
                                    break;
                                }
                            case "Форм-фактор материнської плати":
                                {
                                    List<string> formFactors = value
                                        .Split(new[] { '/', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim())
                                        .ToList();
                                    foreach (var formFactorName in formFactors)
                                    {
                                        string newFormFactorName = formFactorName.ToLower();
                                        if (newFormFactorName == "extended atx" || newFormFactorName == "e-atx" || newFormFactorName == "eatx")
                                        {
                                            newFormFactorName = "E-ATX";
                                        }
                                        else if (newFormFactorName == "microatx" || newFormFactorName == "micro-atx" || newFormFactorName == "micro atx")
                                        {
                                            newFormFactorName = "Micro-ATX";
                                        }
                                        else if (newFormFactorName == "mini-itx" || newFormFactorName == "mini itx" || newFormFactorName == "miniitx")
                                        {
                                            newFormFactorName = "Mini-ITX";
                                        }
                                        else if (newFormFactorName == "atx")
                                        {
                                            newFormFactorName = "ATX";
                                        }
                                        else
                                        {
                                            newFormFactorName = formFactorName;
                                        }
                                        pcCase.PcCaseFormFactors.Add(new PcCaseFormFactor
                                        {
                                            Name = newFormFactorName
                                        });

                                    }
                                    break;
                                }
                            case "Місця під додаткові вентилятори/розташування":
                                {
                                    List<string> fanPlaces = value
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim())
                                        .ToList();

                                    foreach (var place in fanPlaces)
                                    {
                                        string placeLocation;
                                        string placeTypes;

                                        // Format: "Location: sizes"  e.g. "Передня панель: 2x 120 мм/2x 140 мм"
                                        var colonIdx = place.IndexOf(':');
                                        if (colonIdx > 0 && !Regex.IsMatch(place.Substring(0, colonIdx), @"\d"))
                                        {
                                            placeLocation = place.Substring(0, colonIdx).Trim();
                                            placeTypes = place.Substring(colonIdx + 1).Trim();
                                        }
                                        // Format: "sizes (Location)"  e.g. "2х120 мм (передня панель)"
                                        else if (Regex.IsMatch(place, @"\(([^)]+)\)"))
                                        {
                                            var locMatch = Regex.Match(place, @"\(([^)]+)\)");
                                            placeLocation = locMatch.Groups[1].Value.Trim();
                                            placeTypes = Regex.Replace(place, @"\s*\([^)]+\)", "").Trim();
                                        }
                                        else
                                        {
                                            // Format: "Location/sizes" or "sizes/Location"  e.g. "3х120мм / на передній панелі"
                                            var parts = place.Split('/', StringSplitOptions.RemoveEmptyEntries)
                                                             .Select(p => p.Trim())
                                                             .ToList();

                                            placeLocation = string.Join(" / ", parts.Where(p => !Regex.IsMatch(p, @"\d")));
                                            placeTypes = string.Join(" / ", parts.Where(p => Regex.IsMatch(p, @"\d")));

                                            // Format: "Location sizes"  e.g. "нижня панель 2x120 мм"
                                            if (string.IsNullOrEmpty(placeLocation) && parts.Count == 1)
                                            {
                                                var sizeStart = Regex.Match(place, @"\d+\s*[xхXХ]?\s*\d+");
                                                if (sizeStart.Success && sizeStart.Index > 0)
                                                {
                                                    placeLocation = place.Substring(0, sizeStart.Index).Trim();
                                                    placeTypes = place.Substring(sizeStart.Index).Trim();
                                                }
                                            }
                                        }

                                        if (string.IsNullOrEmpty(placeLocation))
                                        {
                                            if (string.IsNullOrEmpty(value))
                                            {
                                                pcCase.BuiltInFans = new LocalizedString { Uk = null };
                                                continue;
                                            }
                                            pcCase.BuiltInFans = new LocalizedString { Uk = char.ToUpper(value[0]) + value.Substring(1) };
                                            continue;
                                        }

                                        if (placeLocation.Contains("живлення"))
                                            placeLocation = "кришка блока живлення";

                                        placeLocation = char.ToUpper(placeLocation[0]) + placeLocation.Substring(1);

                                        // Split by "або" or "/" to get individual size options
                                        var fanTypes = Regex.Split(placeTypes, @"\s*або\s*|\s*/\s*")
                                            .Select(s => s.Trim())
                                            .Where(s => !string.IsNullOrEmpty(s))
                                            .ToList();

                                        foreach (var fanType in fanTypes)
                                        {
                                            var matches = Regex.Matches(fanType, @"(?<count>\d+)\s*[xхXХ]?\s*((?<size>\d+)\s*(мм|mm)?\s*)+", RegexOptions.IgnoreCase);

                                            foreach (Match match in matches)
                                            {
                                                var count = int.Parse(match.Groups["count"].Value);

                                                foreach (Capture sizeCapture in match.Groups["size"].Captures)
                                                {
                                                    if (int.TryParse(sizeCapture.Value, out var size))
                                                    {
                                                        pcCase.PcCaseFanLocations.Add(new PcCaseFanLocation
                                                        {
                                                            Name = new LocalizedString { Uk = placeLocation },
                                                            FanSize = size,
                                                            MaxFans = count
                                                        });
                                                    }
                                                    else
                                                    {
                                                        if (string.IsNullOrEmpty(value))
                                                        {
                                                            pcCase.BuiltInFans = new LocalizedString { Uk = null };
                                                            continue;
                                                        }
                                                        pcCase.BuiltInFans = new LocalizedString { Uk = char.ToUpper(value[0]) + value.Substring(1) };
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                    var existingPcCase = componentsFromDb.FirstOrDefault(s => s.Name == pcCase.Name);
                    if (existingPcCase != null)
                    {
                        pcCase.Id = existingPcCase.Id;
                    }
                    else
                    {
                        pcCase.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<PcCase>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<PcCase>(null, new List<Store>(), new List<ProductOffer>());
            }



            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                pcCase.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = ComponentType.PcCase,
                            ComponentId = pcCase.Id,
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
                pcCase.AveragePrice = (decimal)avgPrice;
                pcCase.OffersCount = offers.Count;
            }


            return new ScrapingResult<PcCase>(pcCase, stores, offers);
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
