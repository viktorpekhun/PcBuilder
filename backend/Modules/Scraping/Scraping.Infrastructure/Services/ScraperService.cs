using Components.Domain.Entities;
using Components.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notifications.Application.Commands;
using Notifications.Domain;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Persistence;
using Scraping.Application.Interfaces;
using Scraping.Infrastructure.Scrapers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Scraping.Infrastructure.Services
{
    public class ScraperService
    {
        private readonly ComponentScraperFactory _scraperFactory;
        private readonly PriceScraperFactory _priceScraperFactory;
        private readonly IPaginationScraper _paginationScraper;
        private readonly IApplicationDbContext _context;
        private readonly IProxyScraper _proxyScraper;
        private readonly ITranslationService _translationService;
        private readonly ProxyPool _proxyPool;
        private readonly ILogger<ScraperService> _logger;
        private readonly ISender _sender;
        private readonly IComponentImageService _imageService;

        private readonly object _lock = new();
        private static readonly SemaphoreSlim _throttle = new(10, 10);

        public ScraperService(ComponentScraperFactory scraperFactory, PriceScraperFactory priceScraperFactory, IPaginationScraper paginationScraper, IApplicationDbContext context, IProxyScraper proxyScraper, ITranslationService translationService, ProxyPool proxyPool, ILogger<ScraperService> logger, ISender sender, IComponentImageService imageService)
        {
            _scraperFactory = scraperFactory;
            _priceScraperFactory = priceScraperFactory;
            _paginationScraper = paginationScraper;
            _context = context;
            _proxyScraper = proxyScraper;
            _translationService = translationService;
            _proxyPool = proxyPool;
            _logger = logger;
            _sender = sender;
            _imageService = imageService;
        }

        private (HttpClient client, string proxy)? CreateHttpClientWithProxy()
        {
            var proxy = _proxyPool.RentProxy();
            if (proxy == null)
                return null;

            var proxyAddress = proxy.Contains("://") ? proxy : $"http://{proxy}";
            var handler = new SocketsHttpHandler
            {
                Proxy = new WebProxy(proxyAddress, false),
                UseProxy = true,
                ConnectTimeout = TimeSpan.FromSeconds(10),
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = 2,
                SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = (_, _, _, _) => true
                }
            };

            var client = new HttpClient(handler, disposeHandler: true)
            {
                Timeout = TimeSpan.FromSeconds(25)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd(Utilities.UserAgentRotator.GetRandom());
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Referrer = new Uri("https://hotline.ua/");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            return (client, proxy);
        }

        private List<string> GetIncludePropertiesForCollections<T>()
        {
            var includeProperties = new List<string>();

            foreach (var property in typeof(T).GetProperties())
            {
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                {
                    includeProperties.Add(property.Name);
                }
            }

            return includeProperties;
        }

        //private async Task TranslateDescriptionsAsync<T>(IEnumerable<T> componentsToSave, IReadOnlyList<T> componentsFromDb, CancellationToken cancellationToken = default) where T : class
        //{
        //    var descriptionProperty = typeof(T).GetProperty("Description");
        //    if (descriptionProperty == null || descriptionProperty.PropertyType != typeof(LocalizedDescription))
        //        return;

        //    var nameProperty = typeof(T).GetProperty("Name");

        //    var toTranslate = new List<(T component, LocalizedDescription desc)>();

        //    foreach (var component in componentsToSave)
        //    {
        //        var desc = descriptionProperty.GetValue(component) as LocalizedDescription;
        //        if (desc == null || string.IsNullOrWhiteSpace(desc.Uk))
        //            continue;

        //        var componentName = nameProperty?.GetValue(component) as string;
        //        var existingComponent = componentsFromDb.FirstOrDefault(c =>
        //        {
        //            var name = nameProperty?.GetValue(c) as string;
        //            return name == componentName;
        //        });

        //        if (existingComponent != null)
        //        {
        //            var existingDesc = descriptionProperty.GetValue(existingComponent) as LocalizedDescription;
        //            if (existingDesc != null && !string.IsNullOrEmpty(existingDesc.En))
        //            {
        //                desc.En = existingDesc.En;
        //                continue;
        //            }
        //        }

        //        toTranslate.Add((component, desc));
        //    }

        //    if (toTranslate.Count == 0)
        //        return;

        //    var ukTexts = toTranslate.Select(x => x.desc.Uk).ToList();
        //    var enTexts = await _translationService.TranslateBatchAsync(ukTexts, "uk", "en", cancellationToken);

        //    for (int i = 0; i < Math.Min(toTranslate.Count, enTexts.Count); i++)
        //    {
        //        if (!string.IsNullOrEmpty(enTexts[i]))
        //            toTranslate[i].desc.En = enTexts[i];
        //    }
        //}

        private async Task UploadComponentImagesAsync<T>(
            IEnumerable<T> componentsToSave,
            IReadOnlyList<T> componentsFromDb,
            string componentType,
            CancellationToken cancellationToken) where T : class
        {
            var photoUrlProp = typeof(T).GetProperty("PhotoUrl");
            var idProp = typeof(T).GetProperty("Id");
            var nameProp = typeof(T).GetProperty("Name");
            if (photoUrlProp == null || idProp == null || nameProp == null) return;

            var existingBlobUrlByName = componentsFromDb
                .ToDictionary(
                    c => nameProp.GetValue(c) as string ?? string.Empty,
                    c => photoUrlProp.GetValue(c) as string);

            foreach (var component in componentsToSave)
            {
                if (idProp.GetValue(component) is not Guid id) continue;
                var name = nameProp.GetValue(component) as string ?? string.Empty;
                var scrapedPhotoUrl = photoUrlProp.GetValue(component) as string;

                if (string.IsNullOrEmpty(scrapedPhotoUrl)) continue;

                var blobUrl = await _imageService.UploadComponentImageAsync(scrapedPhotoUrl, componentType, id, cancellationToken);
                if (blobUrl != null)
                    photoUrlProp.SetValue(component, blobUrl);
            }
        }

        private async Task UploadStoreLogosAsync(
            IEnumerable<Store> storesToSave,
            CancellationToken cancellationToken)
        {
            foreach (var store in storesToSave)
            {
                if (string.IsNullOrEmpty(store.LogoUrl)) continue;

                var blobUrl = await _imageService.UploadStoreLogoAsync(store.LogoUrl, store.Id, cancellationToken);
                if (blobUrl != null)
                    store.LogoUrl = blobUrl;
            }
        }

        public async Task<int> ScrapeCategoryAsync<T>(string categoryUrl, ComponentType componentType, CancellationToken cancellationToken = default) where T : class
        {
            var totalStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting category scraping {ComponentType} from {Url}", componentType, categoryUrl);

            var scraper = _scraperFactory.GetScraper<T>();
            if (scraper == null)
            {
                _logger.LogWarning("Scraper for type {ComponentType} not found", componentType);
                return 0;
            }

            var productLinks = (await _paginationScraper.GetComponentLinksAsync(categoryUrl, cancellationToken))
                .Distinct().ToList();
            _logger.LogInformation("Found {Count} products in category {ComponentType}", productLinks.Count, componentType);

            var failedLinks = new List<string>(productLinks);
            var storesByName = new Dictionary<string, Store>();

            int outerRetry = 0;
            const int maxOuterRetries = 3;

            while (failedLinks.Any() && outerRetry < maxOuterRetries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                outerRetry++;
                Console.WriteLine($"\nProcessing cycle {outerRetry}/{maxOuterRetries}: {failedLinks.Count} links...");
                var successfulProxies = new ConcurrentDictionary<string, bool>();
                var failedProxies = new ConcurrentDictionary<string, bool>();

                if (_proxyPool.NeedsRefresh())
                {
                    Console.WriteLine("Loading and validating new proxies...");
                    var rawProxies = await _proxyScraper.GetProxiesAsync(cancellationToken);

                    if (rawProxies.Count == 0)
                    {
                        Console.WriteLine("Failed to load any proxies. Exiting.");
                        return 0;
                    }

                    await _proxyPool.LoadAndValidateAsync(rawProxies, cancellationToken);

                    if (_proxyPool.AvailableCount == 0)
                    {
                        Console.WriteLine("No proxies passed validation. Exiting.");
                        return 0;
                    }
                }

                var componentsToSave = new ConcurrentBag<T>();
                var storesToSave = new ConcurrentBag<Store>();
                var offersToSave = new ConcurrentBag<ProductOffer>();
                var linksToRetry = new ConcurrentBag<string>();
                var includeProperties = GetIncludePropertiesForCollections<T>();
                IQueryable<T> query = _context.Set<T>().AsQueryable();
                foreach (var prop in includeProperties)
                {
                    query = query.Include(prop);
                }
                var componentsFromDb = await query.ToListAsync(cancellationToken);
                ConcurrentBag<T> concurrentComponents = new ConcurrentBag<T>(componentsFromDb);
                var existingStoresFromDb = await _context.Set<Store>().ToListAsync(cancellationToken);
                ConcurrentBag<Store> concurrentStores = new ConcurrentBag<Store>(existingStoresFromDb);

                var tasks = failedLinks.Select(async (link, index) =>
                {
                    await _throttle.WaitAsync(cancellationToken);
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int maxRetries = 5;
                        int attempt = 0;

                        while (attempt < maxRetries && !cancellationToken.IsCancellationRequested)
                        {
                            string? currentProxy = null;
                            try
                            {
                                var proxyResult = CreateHttpClientWithProxy();
                                if (proxyResult == null)
                                {
                                    Console.WriteLine($"[{index}] No proxies available, skipping attempt.");
                                    attempt++;
                                    await Task.Delay(Random.Shared.Next(1000, 2000) * (attempt + 1), cancellationToken);
                                    continue;
                                }
                                var (client, proxy) = proxyResult.Value;
                                currentProxy = proxy;
                                using (client)
                                {
                                    var result = await scraper.ScrapeAsync(link, client, concurrentComponents, concurrentStores, cancellationToken);

                                    if (result.Component != null)
                                    {
                                        Console.WriteLine($"[{index}] Got component: {result.Component}");
                                        Console.WriteLine($"  Link: {link}");
                                        Console.WriteLine($"  Stores: {result.Stores.Count}, Offers: {result.Offers.Count}");

                                        foreach (var prop in result.Component.GetType().GetProperties())
                                        {
                                            var value = prop.GetValue(result.Component);
                                            Console.Write($"  {prop.Name}: {value}");
                                        }
                                        Console.WriteLine("\n");

                                        componentsToSave.Add(result.Component);

                                        lock (_lock)
                                        {
                                            foreach (var store in result.Stores)
                                            {
                                                if (!storesByName.TryGetValue(store.Name, out var existingStore))
                                                {
                                                    storesByName[store.Name] = store;
                                                    storesToSave.Add(store);
                                                }
                                                else
                                                {
                                                    foreach (var offer in result.Offers.Where(o => o.StoreId == store.Id))
                                                    {
                                                        offer.StoreId = existingStore.Id;
                                                    }
                                                }
                                            }

                                            foreach (var offer in result.Offers)
                                            {
                                                offersToSave.Add(offer);
                                            }
                                        }

                                        _proxyPool.ReturnProxy(proxy, success: true);
                                        successfulProxies.TryAdd(proxy, true);

                                        return;
                                    }
                                    else
                                    {
                                        _proxyPool.ReturnProxy(proxy, success: false);
                                        failedProxies.TryAdd(proxy, true);
                                    }

                                    Console.WriteLine($"[{index}] Parsing failed for {link}");
                                }
                            }
                            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                            {
                                if (currentProxy != null)
                                    _proxyPool.ReturnProxy(currentProxy, success: false);
                                throw;
                            }
                            catch (Exception ex)
                            {
                                if (currentProxy != null)
                                    _proxyPool.ReturnProxy(currentProxy, success: false);
                                Console.WriteLine($"[{index}] Attempt {attempt + 1}/{maxRetries} failed: {ex.Message}");
                            }

                            attempt++;
                            await Task.Delay(Random.Shared.Next(1000, 2000) * (attempt + 1), cancellationToken);
                        }

                        linksToRetry.Add(link);
                    }
                    finally
                    {
                        _throttle.Release();
                        await Task.Delay(Random.Shared.Next(300, 800));
                    }
                });

                await Task.WhenAll(tasks);

                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await UploadComponentImagesAsync(componentsToSave, componentsFromDb, componentType.ToString(), cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception ex) { _logger.LogWarning(ex, "Image upload step failed for {ComponentType}; continuing with DB save", componentType); }

                try
                {
                    await UploadStoreLogosAsync(storesToSave, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception ex) { _logger.LogWarning(ex, "Store logo upload step failed for {ComponentType}; continuing with DB save", componentType); }

                try
                {
                    Console.WriteLine("Saving components to database...");

                    //try
                    //{
                    //    await TranslateDescriptionsAsync(componentsToSave, componentsFromDb, cancellationToken);
                    //}
                    //catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    //{
                    //    throw;
                    //}
                    //catch (Exception ex)
                    //{
                    //    _logger.LogWarning(ex, "Помилка перекладу описів");
                    //}

                    var nameProperty = typeof(T).GetProperty("Name");
                    var idProperty = typeof(T).GetProperty("Id");

                    var offersCountFilterProp = typeof(T).GetProperty("OffersCount");
                    if (offersCountFilterProp != null && idProperty != null)
                    {
                        var withOffers = componentsToSave
                            .Where(c => offersCountFilterProp.GetValue(c) is int count && count > 0)
                            .ToList();
                        var skipped = componentsToSave.Count() - withOffers.Count;
                        if (skipped > 0)
                            _logger.LogInformation("Skipping {Count} {Type} components with 0 offers", skipped, typeof(T).Name);
                        var allowedIds = withOffers
                            .Select(c => idProperty.GetValue(c))
                            .OfType<Guid>()
                            .ToHashSet();
                        componentsToSave = new ConcurrentBag<T>(withOffers);
                        offersToSave = new ConcurrentBag<ProductOffer>(
                            offersToSave.Where(o => allowedIds.Contains(o.ComponentId)));
                    }

                    if (nameProperty != null && idProperty != null)
                    {
                        var keeperIdByName = new Dictionary<string, Guid>();

                        foreach (var existing in componentsFromDb)
                        {
                            var name = nameProperty.GetValue(existing) as string;
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            if (idProperty.GetValue(existing) is not Guid existingId) continue;
                            keeperIdByName[name] = existingId;
                        }

                        foreach (var component in componentsToSave)
                        {
                            var name = nameProperty.GetValue(component) as string;
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            if (idProperty.GetValue(component) is not Guid scrapedId) continue;
                            if (!keeperIdByName.ContainsKey(name))
                                keeperIdByName[name] = scrapedId;
                        }

                        var idRemap = new Dictionary<Guid, Guid>();
                        foreach (var component in componentsToSave)
                        {
                            var name = nameProperty.GetValue(component) as string;
                            if (string.IsNullOrWhiteSpace(name)) continue;
                            if (idProperty.GetValue(component) is not Guid scrapedId) continue;
                            if (keeperIdByName.TryGetValue(name, out var keeperId) && keeperId != scrapedId)
                                idRemap[scrapedId] = keeperId;
                        }

                        if (idRemap.Count > 0)
                        {
                            foreach (var offer in offersToSave)
                            {
                                if (idRemap.TryGetValue(offer.ComponentId, out var keeperId))
                                    offer.ComponentId = keeperId;
                            }
                            _logger.LogInformation("Merged {Count} duplicate-named components for {Type}", idRemap.Count, typeof(T).Name);
                        }
                    }

                    int savedComponentsCount = 0;
                    int updatedComponentsCount = 0;
                    var processedNames = new HashSet<string>();
                    foreach (var component in componentsToSave)
                    {
                        var componentName = nameProperty?.GetValue(component) as string;

                        if (componentName != null && !processedNames.Add(componentName))
                            continue;

                        var existingComponent = componentsFromDb.FirstOrDefault(c =>
                        {
                            var name = nameProperty?.GetValue(c) as string;
                            return name == componentName;
                        });

                        if (existingComponent != null)
                        {
                            foreach (var property in typeof(T).GetProperties())
                            {
                                if (!property.CanWrite || property.Name == "Id")
                                    continue;

                                var newValue = property.GetValue(component);

                                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType)
                                    && property.PropertyType != typeof(string))
                                {
                                    var existingCollection = property.GetValue(existingComponent) as System.Collections.IEnumerable;
                                    var newCollection = newValue as System.Collections.IEnumerable;

                                    if (existingCollection != null && newCollection != null)
                                    {
                                        try
                                        {
                                            var elementType = property.PropertyType.GetGenericArguments().FirstOrDefault();
                                            if (elementType == null) continue;

                                            foreach (var oldItem in existingCollection.Cast<object>().ToList())
                                            {
                                                _context.Remove(oldItem);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error removing old items: {ex.Message}");
                                        }

                                        var targetList = property.GetValue(existingComponent) as System.Collections.IList;
                                        targetList?.Clear();

                                        foreach (var newItem in newCollection)
                                        {
                                            targetList?.Add(newItem);
                                        }
                                    }
                                }
                                //else if (property.PropertyType == typeof(LocalizedDescription)
                                //    && property.GetValue(existingComponent) is LocalizedDescription existingLocalized
                                //    && newValue is LocalizedDescription newLocalized)
                                //{
                                //    existingLocalized.Uk = newLocalized.Uk;
                                //    existingLocalized.En = newLocalized.En;
                                //}
                                else
                                {
                                    property.SetValue(existingComponent, newValue);
                                }
                            }

                            _context.Set<T>().Update(existingComponent);
                            updatedComponentsCount++;
                        }
                        else
                        {
                            await _context.Set<T>().AddAsync(component, cancellationToken);
                            savedComponentsCount++;
                        }
                    }

                    int savedStoresCount = 0;
                    int updatedStoresCount = 0;
                    foreach (var store in storesToSave)
                    {
                        var existingStore = await _context.Set<Store>().FirstOrDefaultAsync(s => s.Id == store.Id, cancellationToken);
                        if (existingStore != null)
                        {
                            existingStore.Name = store.Name;
                            existingStore.LogoUrl = store.LogoUrl;
                            existingStore.Likes = store.Likes;
                            existingStore.Dislikes = store.Dislikes;
                            _context.Set<Store>().Update(existingStore);
                            updatedStoresCount++;
                        }
                        else
                        {
                            await _context.Set<Store>().AddAsync(store, cancellationToken);
                            savedStoresCount++;
                        }
                    }

                    int savedOffersCount = 0;
                    int updatedOffersCount = 0;
                    var offerComponentIds = offersToSave.Select(o => o.ComponentId).Distinct().ToList();
                    var existingOffers = await _context.Set<ProductOffer>()
                        .Where(o => offerComponentIds.Contains(o.ComponentId))
                        .ToListAsync(cancellationToken);
                    var existingOffersLookup = existingOffers
                        .ToDictionary(o => (o.ComponentId, o.StoreId, o.ProductOfferUrl));
                    foreach (var offer in offersToSave)
                    {
                        if (existingOffersLookup.TryGetValue((offer.ComponentId, offer.StoreId, offer.ProductOfferUrl), out var existingOffer))
                        {
                            existingOffer.Price = offer.Price;
                            _context.Set<ProductOffer>().Update(existingOffer);
                            updatedOffersCount++;
                        }
                        else
                        {
                            await _context.Set<ProductOffer>().AddAsync(offer, cancellationToken);
                            savedOffersCount++;
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    Console.WriteLine($"Saved: {savedComponentsCount} components, {savedStoresCount} stores, {savedOffersCount} offers");
                    Console.WriteLine($"Updated: {updatedComponentsCount} components, {updatedStoresCount} stores, {updatedOffersCount} offers");

                    try
                    {
                        var today = DateOnly.FromDateTime(DateTime.UtcNow);
                        var offersByComponent = offersToSave
                            .GroupBy(o => o.ComponentId)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        if (offersByComponent.Count > 0)
                        {
                            var componentIds = offersByComponent.Keys.ToList();
                            var idProp = typeof(T).GetProperty("Id");
                            var averagePriceProp = typeof(T).GetProperty("AveragePrice");
                            var offersCountProp = typeof(T).GetProperty("OffersCount");

                            var allComponents = await _context.Set<T>().ToListAsync(cancellationToken);
                            var componentById = allComponents
                                .Where(c => idProp?.GetValue(c) is Guid)
                                .ToDictionary(c => (Guid)idProp!.GetValue(c)!);

                            var existingHistory = await _context.Set<PriceHistoryEntry>()
                                .Where(h => componentIds.Contains(h.ComponentId)
                                         && h.ComponentType == componentType
                                         && h.SnapshotDate == today)
                                .ToListAsync(cancellationToken);
                            var historyLookup = existingHistory.ToDictionary(h => h.ComponentId);

                            var newAverages = new Dictionary<Guid, decimal>();
                            foreach (var kvp in offersByComponent)
                            {
                                var cid = kvp.Key;
                                var offers = kvp.Value;

                                if (!componentById.TryGetValue(cid, out var comp)) continue;

                                var avg = averagePriceProp?.GetValue(comp) is decimal a ? a : 0m;
                                newAverages[cid] = avg;

                                if (historyLookup.TryGetValue(cid, out var existingHist))
                                {
                                    existingHist.AveragePrice = avg;
                                    existingHist.OffersCount = offers.Count;
                                    _context.Set<PriceHistoryEntry>().Update(existingHist);
                                }
                                else
                                {
                                    await _context.Set<PriceHistoryEntry>().AddAsync(new PriceHistoryEntry
                                    {
                                        Id = Guid.NewGuid(),
                                        ComponentId = cid,
                                        ComponentType = componentType,
                                        AveragePrice = avg,
                                        OffersCount = offers.Count,
                                        SnapshotDate = today
                                    }, cancellationToken);
                                }
                            }

                            await _context.SaveChangesAsync(cancellationToken);

                            _logger.LogInformation("[PriceAlert] Scraped {Count} components with offers for {ComponentType}. ComponentIds: {Ids}",
                                componentIds.Count, componentType, string.Join(", ", componentIds));

                            var subscriptions = await _context.Set<PriceAlertSubscription>()
                                .Where(s => componentIds.Contains(s.ComponentId) && s.ComponentType == componentType)
                                .ToListAsync(cancellationToken);

                            _logger.LogInformation("[PriceAlert] Found {Count} subscriptions matching scraped components ({ComponentType})",
                                subscriptions.Count, componentType);

                            var nameProp = typeof(T).GetProperty("Name");
                            int alertsFired = 0;

                            foreach (var sub in subscriptions)
                            {
                                if (!newAverages.TryGetValue(sub.ComponentId, out var newAvg))
                                {
                                    _logger.LogWarning("[PriceAlert] Subscription {SubId}: ComponentId {CId} not in newAverages — skipping", sub.Id, sub.ComponentId);
                                    continue;
                                }
                                if (sub.LastNotifiedPrice <= 0 || newAvg <= 0)
                                {
                                    _logger.LogWarning("[PriceAlert] Subscription {SubId}: price guard failed (LastNotifiedPrice={Last}, newAvg={New}) — skipping", sub.Id, sub.LastNotifiedPrice, newAvg);
                                    continue;
                                }

                                var deltaPercent = Math.Abs(newAvg - sub.LastNotifiedPrice) / sub.LastNotifiedPrice * 100m;
                                _logger.LogInformation("[PriceAlert] Subscription {SubId}: LastNotified={Last}, NewAvg={New}, Delta={Delta:F2}%, Threshold={Thr}%",
                                    sub.Id, sub.LastNotifiedPrice, newAvg, deltaPercent, sub.ThresholdPercent);

                                if (deltaPercent < sub.ThresholdPercent)
                                {
                                    _logger.LogInformation("[PriceAlert] Subscription {SubId}: delta {Delta:F2}% < threshold {Thr}% — no alert", sub.Id, deltaPercent, sub.ThresholdPercent);
                                    continue;
                                }

                                var direction = newAvg >= sub.LastNotifiedPrice ? "up" : "down";
                                componentById.TryGetValue(sub.ComponentId, out var comp);
                                var name = comp is not null ? (nameProp?.GetValue(comp) as string ?? string.Empty) : string.Empty;

                                await _sender.Send(new CreateNotificationCommand(
                                    sub.UserId,
                                    NotificationTypes.PriceAlert,
                                    new Dictionary<string, string>
                                    {
                                        ["componentId"] = sub.ComponentId.ToString(),
                                        ["componentType"] = componentType.ToString(),
                                        ["componentName"] = name,
                                        ["oldPrice"] = sub.LastNotifiedPrice.ToString(CultureInfo.InvariantCulture),
                                        ["newPrice"] = newAvg.ToString(CultureInfo.InvariantCulture),
                                        ["direction"] = direction,
                                        ["thresholdPercent"] = sub.ThresholdPercent.ToString(CultureInfo.InvariantCulture)
                                    }), cancellationToken);

                                sub.LastNotifiedPrice = newAvg;
                                alertsFired++;
                                _logger.LogInformation("[PriceAlert] Fired alert for subscription {SubId} (user {UserId}, component {CId})", sub.Id, sub.UserId, sub.ComponentId);
                            }

                            if (alertsFired > 0)
                            {
                                await _context.SaveChangesAsync(cancellationToken);
                                _logger.LogInformation("[PriceAlert] Sent {Count} price alert notifications ({ComponentType})", alertsFired, componentType);
                            }
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                    catch (Exception ex) { _logger.LogError(ex, "Error processing price history and notifications"); }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Save cancelled");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving data");
                }

                failedLinks = linksToRetry?.ToList() ?? new List<string>();

                _logger.LogInformation("Cycle {Cycle}/{MaxCycles}: {Available} proxies available, {Success} successful, {Failed} failed. {Remaining} links remaining.",
                    outerRetry, maxOuterRetries, _proxyPool.AvailableCount, successfulProxies.Count, failedProxies.Count, failedLinks.Count);
            }

            totalStopwatch.Stop();
            int totalSuccessful = productLinks.Count - failedLinks.Count;
            double successRate = productLinks.Count > 0 ? (double)totalSuccessful / productLinks.Count * 100 : 0;
            _logger.LogInformation("Scraping {ComponentType} completed in {Duration:F1}s. Successful: {Successful}/{Total} ({SuccessRate:F1}%)",
                componentType, totalStopwatch.Elapsed.TotalSeconds, totalSuccessful, productLinks.Count, successRate);
            return totalSuccessful;
        }

        public async Task<int> UpdatePricesAsync<T>(ComponentType componentType, CancellationToken cancellationToken = default) where T : class
        {
            var totalStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting price update for {ComponentType}", componentType);

            var priceScraper = _priceScraperFactory.GetScraper<T>();
            if (priceScraper == null)
            {
                _logger.LogWarning("Price scraper for type {ComponentType} not found", componentType);
                return 0;
            }

            var hotlineUrlProp = typeof(T).GetProperty("HotlineUrl");
            var idProp = typeof(T).GetProperty("Id");
            var averagePriceProp = typeof(T).GetProperty("AveragePrice");
            var offersCountProp = typeof(T).GetProperty("OffersCount");

            if (hotlineUrlProp == null || idProp == null)
            {
                _logger.LogWarning("Type {ComponentType} does not have HotlineUrl or Id properties", componentType);
                return 0;
            }

            var componentsFromDb = await _context.Set<T>().ToListAsync(cancellationToken);

            var targets = componentsFromDb
                .Select(c => (Component: c,
                              Id: (Guid)(idProp.GetValue(c) ?? Guid.Empty),
                              Url: hotlineUrlProp.GetValue(c) as string))
                .Where(t => !string.IsNullOrWhiteSpace(t.Url))
                .ToList();

            _logger.LogInformation("Found {Count} components with HotlineUrl for {ComponentType}",
                targets.Count, componentType);

            if (targets.Count == 0)
            {
                _logger.LogInformation("No components to update prices for ({ComponentType})", componentType);
                return 0;
            }

            var componentById = targets.ToDictionary(t => t.Id, t => t.Component);

            var failedTargets = new List<(T Component, Guid Id, string? Url)>(targets);

            int outerRetry = 0;
            const int maxOuterRetries = 3;

            while (failedTargets.Any() && outerRetry < maxOuterRetries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                outerRetry++;
                Console.WriteLine($"\nPrice update cycle {outerRetry}/{maxOuterRetries}: {failedTargets.Count} components...");

                if (_proxyPool.NeedsRefresh())
                {
                    Console.WriteLine("Loading and validating new proxies...");
                    var rawProxies = await _proxyScraper.GetProxiesAsync(cancellationToken);

                    if (rawProxies.Count == 0)
                    {
                        Console.WriteLine("Failed to load any proxies. Exiting.");
                        return 0;
                    }

                    await _proxyPool.LoadAndValidateAsync(rawProxies, cancellationToken);

                    if (_proxyPool.AvailableCount == 0)
                    {
                        Console.WriteLine("No proxies passed validation. Exiting.");
                        return 0;
                    }
                }

                var storesToSave = new ConcurrentBag<Store>();
                var storesByName = new Dictionary<string, Store>();
                var offersByComponent = new ConcurrentDictionary<Guid, List<ProductOffer>>();
                var toRetry = new ConcurrentBag<(T Component, Guid Id, string? Url)>();

                var existingStoresFromDb = await _context.Set<Store>().ToListAsync(cancellationToken);
                var concurrentStores = new ConcurrentBag<Store>(existingStoresFromDb);

                var tasks = failedTargets.Select(async (target, index) =>
                {
                    await _throttle.WaitAsync(cancellationToken);
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        int maxRetries = 5;
                        int attempt = 0;

                        while (attempt < maxRetries && !cancellationToken.IsCancellationRequested)
                        {
                            string? currentProxy = null;
                            try
                            {
                                var proxyResult = CreateHttpClientWithProxy();
                                if (proxyResult == null)
                                {
                                    Console.WriteLine($"[{index}] No proxies available, skipping attempt.");
                                    attempt++;
                                    await Task.Delay(Random.Shared.Next(1000, 2000) * (attempt + 1), cancellationToken);
                                    continue;
                                }

                                var (client, proxy) = proxyResult.Value;
                                currentProxy = proxy;
                                using (client)
                                {
                                    var result = await priceScraper.ScrapeAsync(target.Url!, client, target.Component, concurrentStores, cancellationToken);

                                    if (result.Offers.Count > 0)
                                    {
                                        lock (_lock)
                                        {
                                            foreach (var store in result.Stores)
                                            {
                                                if (!storesByName.TryGetValue(store.Name, out var existingStore))
                                                {
                                                    storesByName[store.Name] = store;
                                                    storesToSave.Add(store);
                                                }
                                                else
                                                {
                                                    foreach (var offer in result.Offers.Where(o => o.StoreId == store.Id))
                                                    {
                                                        offer.StoreId = existingStore.Id;
                                                    }
                                                }
                                            }
                                        }

                                        offersByComponent[target.Id] = result.Offers;
                                        _proxyPool.ReturnProxy(proxy, success: true);
                                        Console.WriteLine($"[{index}] Got {result.Offers.Count} offers for component {target.Id}");
                                        return;
                                    }
                                    else
                                    {
                                        _proxyPool.ReturnProxy(proxy, success: false);
                                        Console.WriteLine($"[{index}] No offers for {target.Url}");
                                    }
                                }
                            }
                            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                            {
                                if (currentProxy != null)
                                    _proxyPool.ReturnProxy(currentProxy, success: false);
                                throw;
                            }
                            catch (Exception ex)
                            {
                                if (currentProxy != null)
                                    _proxyPool.ReturnProxy(currentProxy, success: false);
                                Console.WriteLine($"[{index}] Attempt {attempt + 1}/{maxRetries} failed: {ex.Message}");
                            }

                            attempt++;
                            await Task.Delay(Random.Shared.Next(1000, 2000) * (attempt + 1), cancellationToken);
                        }

                        toRetry.Add(target);
                    }
                    finally
                    {
                        _throttle.Release();
                        await Task.Delay(Random.Shared.Next(300, 800));
                    }
                });

                await Task.WhenAll(tasks);
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await UploadStoreLogosAsync(storesToSave, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception ex) { _logger.LogWarning(ex, "Store logo upload step failed during price update; continuing with DB save"); }

                try
                {
                    Console.WriteLine("Saving updated prices to database...");

                    int savedStoresCount = 0, updatedStoresCount = 0;
                    foreach (var store in storesToSave)
                    {
                        var existingStore = await _context.Set<Store>().FirstOrDefaultAsync(s => s.Id == store.Id, cancellationToken);
                        if (existingStore != null)
                        {
                            existingStore.Name = store.Name;
                            existingStore.LogoUrl = store.LogoUrl;
                            existingStore.Likes = store.Likes;
                            existingStore.Dislikes = store.Dislikes;
                            _context.Set<Store>().Update(existingStore);
                            updatedStoresCount++;
                        }
                        else
                        {
                            await _context.Set<Store>().AddAsync(store, cancellationToken);
                            savedStoresCount++;
                        }
                    }

                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    var componentIds = offersByComponent.Keys.ToList();

                    var existingOffersList = await _context.Set<ProductOffer>()
                        .Where(o => componentIds.Contains(o.ComponentId) && o.ComponentType == componentType)
                        .ToListAsync(cancellationToken);
                    var existingOffersLookup = existingOffersList
                        .ToDictionary(o => (o.ComponentId, o.StoreId, o.ProductOfferUrl));

                    var existingHistory = await _context.Set<PriceHistoryEntry>()
                        .Where(h => componentIds.Contains(h.ComponentId)
                                 && h.ComponentType == componentType
                                 && h.SnapshotDate == today)
                        .ToListAsync(cancellationToken);
                    var historyLookup = existingHistory.ToDictionary(h => h.ComponentId);

                    int savedOffersCount = 0, updatedOffersCount = 0;
                    int componentsUpdated = 0, historyInserted = 0, historyUpdated = 0;

                    foreach (var kvp in offersByComponent)
                    {
                        var componentId = kvp.Key;
                        var offers = kvp.Value;

                        foreach (var offer in offers)
                        {
                            if (existingOffersLookup.TryGetValue((offer.ComponentId, offer.StoreId, offer.ProductOfferUrl), out var existingOffer))
                            {
                                existingOffer.Price = offer.Price;
                                _context.Set<ProductOffer>().Update(existingOffer);
                                updatedOffersCount++;
                            }
                            else
                            {
                                await _context.Set<ProductOffer>().AddAsync(offer, cancellationToken);
                                savedOffersCount++;
                            }
                        }

                        if (componentById.TryGetValue(componentId, out var component))
                        {
                            var avg = offers.Any() ? Math.Round(offers.Average(o => o.Price), 0) : 0m;
                            averagePriceProp?.SetValue(component, avg);
                            offersCountProp?.SetValue(component, offers.Count);
                            _context.Set<T>().Update(component);
                            componentsUpdated++;

                            if (historyLookup.TryGetValue(componentId, out var existingHist))
                            {
                                existingHist.AveragePrice = avg;
                                existingHist.OffersCount = offers.Count;
                                _context.Set<PriceHistoryEntry>().Update(existingHist);
                                historyUpdated++;
                            }
                            else
                            {
                                await _context.Set<PriceHistoryEntry>().AddAsync(new PriceHistoryEntry
                                {
                                    Id = Guid.NewGuid(),
                                    ComponentId = componentId,
                                    ComponentType = componentType,
                                    AveragePrice = avg,
                                    OffersCount = offers.Count,
                                    SnapshotDate = today
                                }, cancellationToken);
                                historyInserted++;
                            }
                        }
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    Console.WriteLine($"Saved: {savedStoresCount} stores, {savedOffersCount} offers, {historyInserted} history records");
                    Console.WriteLine($"Updated: {updatedStoresCount} stores, {updatedOffersCount} offers, {componentsUpdated} components, {historyUpdated} history records");

                    try
                    {
                        var newAverages = offersByComponent
                            .Where(kv => componentById.ContainsKey(kv.Key) && kv.Value.Any())
                            .ToDictionary(kv => kv.Key, kv => Math.Round(kv.Value.Average(o => o.Price), 0));

                        if (newAverages.Count > 0)
                        {
                            var alertComponentIds = newAverages.Keys.ToList();
                            var subscriptions = await _context.Set<PriceAlertSubscription>()
                                .Where(s => alertComponentIds.Contains(s.ComponentId) && s.ComponentType == componentType)
                                .ToListAsync(cancellationToken);

                            var nameProp = typeof(T).GetProperty("Name");
                            int alertsFired = 0;

                            foreach (var sub in subscriptions)
                            {
                                if (!newAverages.TryGetValue(sub.ComponentId, out var newAvg)) continue;
                                if (sub.LastNotifiedPrice <= 0 || newAvg <= 0) continue;

                                var deltaPercent = Math.Abs(newAvg - sub.LastNotifiedPrice) / sub.LastNotifiedPrice * 100m;
                                if (deltaPercent < sub.ThresholdPercent) continue;

                                var direction = newAvg >= sub.LastNotifiedPrice ? "up" : "down";
                                componentById.TryGetValue(sub.ComponentId, out var comp);
                                var name = comp is not null ? (nameProp?.GetValue(comp) as string ?? string.Empty) : string.Empty;

                                await _sender.Send(new CreateNotificationCommand(
                                    sub.UserId,
                                    NotificationTypes.PriceAlert,
                                    new Dictionary<string, string>
                                    {
                                        ["componentId"] = sub.ComponentId.ToString(),
                                        ["componentType"] = componentType.ToString(),
                                        ["componentName"] = name,
                                        ["oldPrice"] = sub.LastNotifiedPrice.ToString(CultureInfo.InvariantCulture),
                                        ["newPrice"] = newAvg.ToString(CultureInfo.InvariantCulture),
                                        ["direction"] = direction,
                                        ["thresholdPercent"] = sub.ThresholdPercent.ToString(CultureInfo.InvariantCulture)
                                    }), cancellationToken);

                                sub.LastNotifiedPrice = newAvg;
                                alertsFired++;
                            }

                            if (alertsFired > 0)
                            {
                                await _context.SaveChangesAsync(cancellationToken);
                                _logger.LogInformation("Sent {Count} price change notifications ({ComponentType})", alertsFired, componentType);
                            }
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing price change notifications");
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Price update save cancelled");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving updated prices");
                }

                failedTargets = toRetry.ToList();

                _logger.LogInformation("Price update cycle {Cycle}/{MaxCycles}. Remaining: {Remaining}",
                    outerRetry, maxOuterRetries, failedTargets.Count);
            }

            totalStopwatch.Stop();
            int totalSuccessful = targets.Count - failedTargets.Count;
            double successRate = targets.Count > 0 ? (double)totalSuccessful / targets.Count * 100 : 0;
            _logger.LogInformation("Price update {ComponentType} completed in {Duration:F1}s. Successful: {Successful}/{Total} ({SuccessRate:F1}%)",
                componentType, totalStopwatch.Elapsed.TotalSeconds, totalSuccessful, targets.Count, successRate);
            return totalSuccessful;
        }

        public async Task ScrapeSingleComponentAsync<T>(string componentUrl, ComponentType componentType, Type[]? NestedTypes = null, CancellationToken cancellationToken = default) where T : class
        {
            cancellationToken.ThrowIfCancellationRequested();

            Console.WriteLine("Starting ScrapeSingleComponentAsync\n");

            var scraper = _scraperFactory.GetScraper<T>();
            if (scraper == null)
            {
                Console.WriteLine("Scraper for this type not found!");
                return;
            }

            var emptyList = new List<T>();
            var emptyStoresList = new List<Store>();
            ConcurrentBag<T> concurrentComponents = new ConcurrentBag<T>(emptyList);
            ConcurrentBag<Store> concurrentStores = new ConcurrentBag<Store>(emptyStoresList);


            var cookieContainer = new CookieContainer();

            var handler = new SocketsHttpHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true,
                AllowAutoRedirect = true,
                SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                {
                    EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
                }
            };

            using var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"");
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");



            var result = await scraper.ScrapeAsync(componentUrl, client, concurrentComponents, concurrentStores, cancellationToken);

            if (result.Component != null)
            {
                try
                {
                    var photoUrlProp = typeof(T).GetProperty("PhotoUrl");
                    var idProp = typeof(T).GetProperty("Id");
                    if (photoUrlProp != null && idProp != null
                        && idProp.GetValue(result.Component) is Guid componentId
                        && photoUrlProp.GetValue(result.Component) is string scrapedPhotoUrl)
                    {
                        var blobUrl = await _imageService.UploadComponentImageAsync(
                            scrapedPhotoUrl, typeof(T).Name, componentId, cancellationToken);
                        if (blobUrl != null)
                            photoUrlProp.SetValue(result.Component, blobUrl);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception ex) { Console.WriteLine($"  Image upload failed: {ex.Message}"); }

                Console.WriteLine($"  Got component: {result.Component}");
                Console.WriteLine($"  Link: {componentUrl}");
                Console.WriteLine($"  Stores: {result.Stores.Count}, Offers: {result.Offers.Count}");

                //try
                //{
                //    await TranslateDescriptionsAsync([result.Component], [], cancellationToken);
                //    Console.WriteLine("  Переклад виконано успішно.");
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"  Помилка перекладу: {ex.Message}");
                //}

                //foreach (var prop in result.Component.GetType().GetProperties())
                //{
                //    var value = prop.GetValue(result.Component);
                //    if(prop.Name == "Description" && value is LocalizedDescription localized)
                //    {
                //        Console.WriteLine($"  {prop.Name} (UK): {localized.Uk}");
                //        //Console.WriteLine($"  {prop.Name} (EN): {localized.En}");
                //    }
                //    else
                //    {
                //        Console.WriteLine($"  {prop.Name}: {value}");
                //    }
                //}
                var allowedTypes = NestedTypes ?? Array.Empty<Type>();
                var objectsToProcess = new Queue<(object Instance, int Level)>();
                objectsToProcess.Enqueue((result.Component, 0));

                while (objectsToProcess.Count > 0)
                {
                    var current = objectsToProcess.Dequeue();
                    var indent = new string(' ', current.Level * 2);

                    foreach (var prop in current.Instance.GetType().GetProperties())
                    {
                        var value = prop.GetValue(current.Instance);
                        if (value == null) continue;

                        var actualType = value.GetType();

                        // 1. LocalizedString — вивести Uk і En
                        if (value is LocalizedString localized)
                        {
                            Console.WriteLine($"{indent}{prop.Name} (UK): {localized.Uk}");
                            Console.WriteLine($"{indent}{prop.Name} (EN): {localized.En}");
                            continue;
                        }

                        // 2. НОВИЙ БЛОК: Якщо це список (колекція), але не рядок
                        if (value is System.Collections.IEnumerable collection && value is not string)
                        {
                            Console.WriteLine($"{indent}{prop.Name} (Collection):");
                            foreach (var item in collection)
                            {
                                if (item != null)
                                    // Додаємо кожен елемент списку в чергу, щоб розібрати його властивості
                                    objectsToProcess.Enqueue((item, current.Level + 1));
                            }
                        }
                        // 3. Якщо це поодинокий вкладений клас із дозволених
                        else if (allowedTypes.Any(t => t.IsAssignableFrom(actualType)))
                        {
                            Console.WriteLine($"{indent}{prop.Name}:");
                            objectsToProcess.Enqueue((value, current.Level + 1));
                        }
                        // 4. Звичайний вивід (текст, цифри тощо)
                        else
                        {
                            Console.WriteLine($"{indent}{prop.Name}: {value}");
                        }
                    }
                }
                Console.WriteLine("\n");
            }
            else
            {
                Console.WriteLine($"  Failed to get component.");
            }
        }
    }
}
