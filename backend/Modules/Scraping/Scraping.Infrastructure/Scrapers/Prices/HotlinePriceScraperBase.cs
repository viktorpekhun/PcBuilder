using Components.Domain.Entities;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application.Interfaces;
using Scraping.Infrastructure.Utilities;
using System.Collections.Concurrent;

namespace Scraping.Infrastructure.Scrapers.Prices
{
    public abstract class HotlinePriceScraperBase<TComponent> : IPriceScraper<TComponent> where TComponent : class
    {
        private const string BaseUrl = "https://hotline.ua";

        protected abstract ComponentType ComponentType { get; }

        public async Task<PriceScrapingResult> ScrapeAsync(
            string url,
            HttpClient client,
            Guid componentId,
            ConcurrentBag<Store> storesFromDb,
            CancellationToken cancellationToken = default)
        {
            var stores = new List<Store>();
            var offers = new List<ProductOffer>();

            var html = await client.GetStringAsync(url, cancellationToken);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nuxtData = NuxtScriptWorker.ExtractNuxtDataFromHtml(htmlDoc);
            if (nuxtData == null)
                return new PriceScrapingResult(stores, offers);

            var resultOffers = NuxtScriptWorker.FindTokenByKey(nuxtData, "offers");
            var edgesOffers = resultOffers?["edges"] as JArray;
            if (edgesOffers == null)
                return new PriceScrapingResult(stores, offers);

            foreach (var edge in edgesOffers)
            {
                try
                {
                    var node = edge["node"];
                    if (node == null) continue;

                    var storeName = node["firmTitle"]?.ToString().Trim();
                    if (string.IsNullOrEmpty(storeName)) continue;

                    var storeLogoUrl = node["firmLogo"]?.ToString().Trim();
                    if (!string.IsNullOrEmpty(storeLogoUrl) && storeLogoUrl.StartsWith("/"))
                    {
                        storeLogoUrl = $"{BaseUrl}{storeLogoUrl}";
                    }

                    int likes = node["reviewsPositiveNumber"]?.Value<int?>() ?? 0;
                    int dislikes = node["reviewsNegativeNumber"]?.Value<int?>() ?? 0;

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
                        store.Id = storeFromDb?.Id ?? Guid.NewGuid();
                        stores.Add(store);
                    }

                    decimal price = node["price"]?.Value<decimal>() ?? 0;

                    var offerUrl = node["conversionUrl"]?.ToString().Trim();
                    if (string.IsNullOrEmpty(offerUrl)) continue;

                    if (offerUrl.StartsWith("/"))
                    {
                        offerUrl = $"{BaseUrl}{offerUrl}";
                    }

                    offers.Add(new ProductOffer
                    {
                        Id = Guid.NewGuid(),
                        Price = price,
                        ComponentType = ComponentType,
                        ComponentId = componentId,
                        ProductOfferUrl = offerUrl,
                        StoreId = store.Id
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scraping offer: {ex.Message}");
                }
            }

            return new PriceScrapingResult(stores, offers);
        }
    }
}
