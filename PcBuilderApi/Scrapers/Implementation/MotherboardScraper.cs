using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text.RegularExpressions;

namespace PcBuilderApi.Scrapers.Implementation
{
    public class MotherboardScraper : IComponentScraper<Motherboard>
    {
        private const string BaseUrl = "https://hotline.ua";
        public async Task<ScrapingResult<Motherboard>> ScrapeAsync(string url, HttpClient client, ConcurrentBag<Motherboard> componentsFromDb, ConcurrentBag<Store> storesFromDb)
        {
            var html = await client.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var motherboard = new Motherboard();
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            string modelInBrackets = "";
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title__main')]");
            if (titleNode != null)
            {
                motherboard.Name = titleNode.InnerText.Trim();
                var match = Regex.Match(motherboard.Name, @"\((.*?)\)");
                if (match.Success)
                {
                    modelInBrackets = match.Groups[1].Value;
                }
                motherboard.Name = Regex.Replace(motherboard.Name, @"\s*\(.*?\)", "");
            }
            else
            {
                return new ScrapingResult<Motherboard>(null, new List<Store>(), new List<ProductOffer>());
            }

            var descriptionNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'description__content')]");
            if (descriptionNode != null)
            {
                motherboard.Description = descriptionNode.InnerText.Trim();
                if (!string.IsNullOrEmpty(modelInBrackets))
                {
                    motherboard.Description = Regex.Replace(motherboard.Description, $@"\({Regex.Escape(modelInBrackets)}\)", "");
                }
            }


            var nuxtData = NuxtScriptWorker.ExtractNuxtDataFromHtml(htmlDoc);
            if (nuxtData != null)
            {
                var resultSpecs = NuxtScriptWorker.FindTokenByKey(nuxtData, "productValues");
                var edgesSpecs = resultSpecs?["edges"] as JArray;
                if (edgesSpecs != null)
                {
                    var rearPorts = new List<RearPort>();
                    var innerPorts = new List<InnerPort>();
                    foreach (var edge in edgesSpecs)
                    {
                        var node = edge["node"];

                        var key = node?["title"]?.ToString().Trim();
                        var value = node?["value"]?.ToString().Trim();

                        switch (key)
                        {
                            case "vendor":
                                motherboard.Brand = value ?? string.Empty;
                                break;
                            case "Тип роз'єму CPU":
                                motherboard.Socket = value ?? string.Empty;
                                break;
                            case "Чіпсет":
                                motherboard.Chipset = value ?? string.Empty;
                                break;
                            case "DIMM":
                                var dimmSlotsMatch = Regex.Match(value, @"(\d+)\s*x", RegexOptions.IgnoreCase);
                                if (dimmSlotsMatch.Success)
                                {
                                    motherboard.DimmSlots = ParseInt(dimmSlotsMatch.Groups[1].Value);
                                }

                                var dimmTypeMatch = Regex.Match(value, @"(DDR\d)", RegexOptions.IgnoreCase);
                                if (dimmTypeMatch.Success)
                                {
                                    motherboard.DimmType = dimmTypeMatch.Groups[1].Value.ToUpper();
                                }

                                var freqMatch = Regex.Match(value, @"(\d{3,5})\+?\s*(МГц)?", RegexOptions.IgnoreCase);
                                if (freqMatch.Success)
                                {
                                    motherboard.DimmFrequency = ParseInt(freqMatch.Groups[1].Value);
                                }

                                var capacityMatch = Regex.Match(value, @"до\s+([\d,.]+)\s*(ГБ|ТБ)", RegexOptions.IgnoreCase);
                                if (capacityMatch.Success)
                                {
                                    var rawNumber = capacityMatch.Groups[1].Value;
                                    var unit = capacityMatch.Groups[2].Value.ToUpper();

                                    var parsed = ParseDouble(rawNumber);
                                    if (parsed.HasValue)
                                    {
                                        var capacityValue = parsed.Value;
                                        if (unit == "ТБ")
                                            capacityValue *= 1024;

                                        motherboard.DimmCapacity = (int)capacityValue;
                                    }
                                }

                                break;
                            case "SATA Revision 3.0":
                                motherboard.Sata3Count = ParseInt(value);
                                break;
                            case "Живлення материнської плати":
                                var powerMatch = Regex.Match(value, @"(\d+)\s*pin", RegexOptions.IgnoreCase);
                                if (powerMatch.Success)
                                {
                                    motherboard.PowerMotherboard = ParseInt(powerMatch.Groups[1].Value);
                                }
                                else
                                {
                                    motherboard.PowerMotherboard = 24;
                                }
                                break;
                            case "FAN":
                                motherboard.FanQuantity = ParseInt(value);
                                break;
                            case "PCI Express x1":
                                motherboard.PcleX1Quantity = ParseInt(value);
                                break;
                            case "Ethernet":
                                if (string.Equals(value, "немає", StringComparison.OrdinalIgnoreCase))
                                    motherboard.Ethernet = string.Empty;
                                else
                                    motherboard.Ethernet = value ?? string.Empty;
                                break;
                            case "Aудіокодек":
                                if (string.Equals(value, "немає", StringComparison.OrdinalIgnoreCase))
                                    motherboard.Audio = string.Empty;
                                else
                                    motherboard.Audio = value ?? string.Empty;
                                break;
                            case "Адаптер Wi-Fi":
                                if (string.Equals(value, "немає", StringComparison.OrdinalIgnoreCase))
                                    motherboard.Wifi = string.Empty;
                                else
                                    motherboard.Wifi = value ?? string.Empty;
                                break;
                            case "Адаптер Bluetooth":
                                if (string.Equals(value, "немає", StringComparison.OrdinalIgnoreCase))
                                    motherboard.Bluetooth = string.Empty;
                                else
                                    motherboard.Bluetooth = value ?? string.Empty;
                                break;
                            case "Виходи для монітора":
                                if (string.Equals(value, "немає", StringComparison.OrdinalIgnoreCase))
                                    motherboard.VideoPorts = string.Empty;
                                else
                                    motherboard.VideoPorts = value ?? string.Empty;
                                break;
                            case "Форм-фактор":
                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    var parts = value.Split(',', 2, StringSplitOptions.TrimEntries);

                                    if (parts.Length == 2)
                                    {
                                        motherboard.FormFactor = parts[0];
                                        motherboard.SizeDimentions = parts[1];
                                    }
                                    else
                                    {
                                        motherboard.FormFactor = value.Trim();
                                    }
                                }
                                break;
                            case "productOnVendorSite":
                                motherboard.FactoryLink = node?["url"]?.ToString().Trim();
                                break;
                            case "Живлення процесора":
                                {
                                    var connectors = new List<CpuPowerConnector>();

                                    var parts = value.Replace("+", ",").Split(',', StringSplitOptions.RemoveEmptyEntries);

                                    foreach (var part in parts)
                                    {
                                        var partMatch = Regex.Match(part.Trim(), @"(\d+)x\s*(\d+)pin", RegexOptions.IgnoreCase);
                                        if (partMatch.Success)
                                        {
                                            var quantity = int.Parse(partMatch.Groups[1].Value);
                                            var pins = int.Parse(partMatch.Groups[2].Value);

                                            connectors.Add(new CpuPowerConnector
                                            {
                                                Quantity = quantity,
                                                Pins = pins
                                            });
                                        }
                                        else
                                        {
                                            connectors.Add(new CpuPowerConnector
                                            {
                                                Quantity = 1,
                                                Pins = 8
                                            });
                                        }
                                    }

                                    motherboard.CpuPowerConnectors = connectors;
                                    break;
                                }
                            case "PCI Express x16":
                                {
                                    value = value.Replace('х', 'x');
                                    var pcleSlots = new List<PcleSlot>();
                                    var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var part in parts)
                                    {
                                        var trimmedPart = part.Trim();

                                        var matchSingle = Regex.Match(trimmedPart, @"(\d+)x(?:PCI[-\s]?E|PCI Express)\s+([\d.]+)[^\(]*\((x\d+)\)", RegexOptions.IgnoreCase);

                                        if (matchSingle.Success)
                                        {
                                            int quantity = int.Parse(matchSingle.Groups[1].Value);
                                            double version = double.Parse(matchSingle.Groups[2].Value, CultureInfo.InvariantCulture);
                                            int lane = int.Parse(matchSingle.Groups[3].Value.TrimStart('x'));

                                            for (int i = 0; i < quantity; i++)
                                            {
                                                pcleSlots.Add(new PcleSlot
                                                {
                                                    Quantity = 1,
                                                    Version = version,
                                                    Lane = lane
                                                });
                                            }

                                            continue;
                                        }

                                        var matchSingleDifferent = Regex.Match(trimmedPart, @"(\d+)x(?:PCI[-\s]?E|PCI Express)\s+([\d.]+)", RegexOptions.IgnoreCase);
                                        if (matchSingleDifferent.Success)
                                        {
                                            int quantity = int.Parse(matchSingleDifferent.Groups[1].Value);
                                            double version = double.Parse(matchSingleDifferent.Groups[2].Value, CultureInfo.InvariantCulture);
                                            for (int i = 0; i < quantity; i++)
                                            {
                                                pcleSlots.Add(new PcleSlot
                                                {
                                                    Quantity = 1,
                                                    Version = version,
                                                    Lane = 16
                                                });
                                            }
                                            continue;
                                        }

                                        trimmedPart = value.Replace(",", "/");
                                        var matchMultiple = Regex.Match(trimmedPart, @"(\d+)x\s*(?:PCI[-\s]?E|PCI Express)\s+([\d.]+)\s+[xX]?\d+\s*\(\s*([xX]?\d+\s*(?:\/\s*[xX]?\d+\s*)*)\)", RegexOptions.IgnoreCase);
                                        if (matchMultiple.Success)
                                        {
                                            int quantity = int.Parse(matchMultiple.Groups[1].Value);
                                            double version = double.Parse(matchMultiple.Groups[2].Value, CultureInfo.InvariantCulture);
                                            var lanesGroup = matchMultiple.Groups[3].Value;

                                            var lanes = lanesGroup.Split('/', StringSplitOptions.RemoveEmptyEntries)
                                                                  .Select(l => l.Trim())
                                                                  .Select(l => int.TryParse(l.TrimStart('x'), out var lane) ? lane : 16)
                                                                  .ToList();

                                            if (lanes.Count == quantity)
                                            {
                                                for (int i = 0; i < quantity; i++)
                                                {
                                                    pcleSlots.Add(new PcleSlot
                                                    {
                                                        Quantity = 1,
                                                        Version = version,
                                                        Lane = lanes[i]
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                for (int i = 0; i < quantity; i++)
                                                {
                                                    int lane = lanes.Count > i ? lanes[i] : lanes.Last();
                                                    pcleSlots.Add(new PcleSlot
                                                    {
                                                        Quantity = 1,
                                                        Version = version,
                                                        Lane = lane
                                                    });
                                                }
                                            }
                                            break;
                                        }
                                    }

                                    motherboard.PcleSlots = pcleSlots;
                                    break;
                                }
                            case "Тип M.2":
                                {
                                    var m2Slots = new List<M2Slot>();

                                    var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                                    foreach (var part in parts)
                                    {
                                        var trimmed = part.Trim();

                                        var matchWithQuantity = Regex.Match(trimmed, @"(\d+)x\s*PCI-E\s*([\d.]+)\s*x(\d+)", RegexOptions.IgnoreCase);

                                        if (matchWithQuantity.Success)
                                        {
                                            int quantity = int.Parse(matchWithQuantity.Groups[1].Value);
                                            double version = double.Parse(matchWithQuantity.Groups[2].Value, CultureInfo.InvariantCulture);
                                            int lane = int.Parse(matchWithQuantity.Groups[3].Value);

                                            m2Slots.Add(new M2Slot
                                            {
                                                Quantity = quantity,
                                                Version = version,
                                                Lane = lane
                                            });
                                        }
                                        else
                                        {
                                            var match = Regex.Match(trimmed, @"PCI-E\s*([\d.]+)\s*x(\d+)", RegexOptions.IgnoreCase);
                                            if (match.Success)
                                            {
                                                double version = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                                                int lane = int.Parse(match.Groups[2].Value);

                                                m2Slots.Add(new M2Slot
                                                {
                                                    Quantity = 1,
                                                    Version = version,
                                                    Lane = lane
                                                });
                                            }
                                        }
                                    }

                                    motherboard.M2Slots = m2Slots;
                                    break;
                                }
                            case "USB 3.2 Gen 2x2 Type-C" or "USB 3.2 Gen 2 Type-C (USB 3.1)" or "USB 3.2 Gen 2 Type-A (USB 3.1)" or "USB 3.2 Gen 1 Type-A (USB 3.0)" or "USB 2.0" or "Thunderbolt" or "LAN" or "Audio":
                                {
                                    var valueCheck = ParseInt(value);
                                    if (valueCheck != null)
                                    {
                                        rearPorts.Add(new RearPort
                                        {
                                            Type = key,
                                            Quantity = valueCheck
                                        });
                                    }
                                    break;
                                }
                            case "USB 3.2 Gen 2" or "USB 3.2 Gen 1" or "USB 2.0 на платі":
                                {
                                    innerPorts.Add(new InnerPort
                                    {
                                        Type = key,
                                        Value = value
                                    });
                                    break;
                                }

                        }
                    }
                    if (motherboard.FormFactor != null && motherboard.FormFactor != "" && motherboard.FormFactor != string.Empty)
                    {
                        switch (motherboard.FormFactor.ToLower())
                        {
                            case "atx":
                                motherboard.Wattage = 70;
                                break;
                            case "e-atx":
                                motherboard.Wattage = 100;
                                break;
                            case "microatx":
                                motherboard.Wattage = 60;
                                break;
                            case "mini-itx":
                                motherboard.Wattage = 30;
                                break;
                            case "xl-atx":
                                motherboard.Wattage = 130;
                                break;
                        }
                    }

                    if (rearPorts.Count > 0)
                    {
                        motherboard.RearPorts = rearPorts;
                    }
                    if (innerPorts.Count > 0)
                    {
                        motherboard.InnerPorts = innerPorts;
                    }

                    var existingMotherboard = componentsFromDb.FirstOrDefault(s => s.Name == motherboard.Name);
                    if (existingMotherboard != null)
                    {
                        motherboard.Id = existingMotherboard.Id;
                    }
                    else
                    {
                        motherboard.Id = Guid.NewGuid();
                    }
                }
                else
                {
                    return new ScrapingResult<Motherboard>(null, new List<Store>(), new List<ProductOffer>());
                }
            }
            else
            {
                return new ScrapingResult<Motherboard>(null, new List<Store>(), new List<ProductOffer>());
            }



            var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img[contains(@class, 'zoom-gallery__canvas-img')]");
            if (imageNode != null)
            {
                string imgSrc = imageNode.GetAttributeValue("src", "");
                motherboard.PhotoUrl = !string.IsNullOrEmpty(imgSrc) ? (imgSrc.StartsWith("/") ? $"{BaseUrl}{imgSrc}" : imgSrc) : null;
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
                            ComponentType = SD.ComponentType.Motherboard,
                            ComponentId = motherboard.Id,
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
                motherboard.AveragePrice = (decimal)avgPrice;
                motherboard.OffersCount = offers.Count;
            }


            return new ScrapingResult<Motherboard>(motherboard, stores, offers);
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