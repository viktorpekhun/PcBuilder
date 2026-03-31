using Components.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Persistence;
using Scraping.Application.Interfaces;
using Scraping.Infrastructure.Scrapers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace Scraping.Infrastructure.Services
{
    public class ScraperService
    {
        private readonly ComponentScraperFactory _scraperFactory;
        private readonly IPaginationScraper _paginationScraper;
        private readonly IApplicationDbContext _context;
        private readonly IProxyScraper _proxyScraper;
        private readonly ITranslationService _translationService;
        private readonly ProxyPool _proxyPool;
        private readonly ILogger<ScraperService> _logger;

        private readonly object _lock = new();
        private static readonly SemaphoreSlim _throttle = new(10, 10);

        public ScraperService(ComponentScraperFactory scraperFactory, IPaginationScraper paginationScraper, IApplicationDbContext context, IProxyScraper proxyScraper, ITranslationService translationService, ProxyPool proxyPool, ILogger<ScraperService> logger)
        {
            _scraperFactory = scraperFactory;
            _paginationScraper = paginationScraper;
            _context = context;
            _proxyScraper = proxyScraper;
            _translationService = translationService;
            _proxyPool = proxyPool;
            _logger = logger;
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

        private async Task TranslateDescriptionsAsync<T>(IEnumerable<T> componentsToSave, IReadOnlyList<T> componentsFromDb, CancellationToken cancellationToken = default) where T : class
        {
            var descriptionProperty = typeof(T).GetProperty("Description");
            if (descriptionProperty == null || descriptionProperty.PropertyType != typeof(LocalizedDescription))
                return;

            var nameProperty = typeof(T).GetProperty("Name");

            var toTranslate = new List<(T component, LocalizedDescription desc)>();

            foreach (var component in componentsToSave)
            {
                var desc = descriptionProperty.GetValue(component) as LocalizedDescription;
                if (desc == null || string.IsNullOrWhiteSpace(desc.Uk))
                    continue;

                var componentName = nameProperty?.GetValue(component) as string;
                var existingComponent = componentsFromDb.FirstOrDefault(c =>
                {
                    var name = nameProperty?.GetValue(c) as string;
                    return name == componentName;
                });

                if (existingComponent != null)
                {
                    var existingDesc = descriptionProperty.GetValue(existingComponent) as LocalizedDescription;
                    if (existingDesc != null && !string.IsNullOrEmpty(existingDesc.En))
                    {
                        desc.En = existingDesc.En;
                        continue;
                    }
                }

                toTranslate.Add((component, desc));
            }

            if (toTranslate.Count == 0)
                return;

            var ukTexts = toTranslate.Select(x => x.desc.Uk).ToList();
            var enTexts = await _translationService.TranslateBatchAsync(ukTexts, "uk", "en", cancellationToken);

            for (int i = 0; i < Math.Min(toTranslate.Count, enTexts.Count); i++)
            {
                if (!string.IsNullOrEmpty(enTexts[i]))
                    toTranslate[i].desc.En = enTexts[i];
            }
        }

        public async Task ScrapeCategoryAsync<T>(string categoryUrl, ComponentType componentType, CancellationToken cancellationToken = default) where T : class
        {
            var totalStopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Початок скрапінгу категорії {ComponentType} з {Url}", componentType, categoryUrl);

            var scraper = _scraperFactory.GetScraper<T>();
            if (scraper == null)
            {
                _logger.LogWarning("Скрейпер для типу {ComponentType} не знайдено", componentType);
                return;
            }

            var productLinks = await _paginationScraper.GetComponentLinksAsync(categoryUrl, cancellationToken);
            _logger.LogInformation("Знайдено {Count} товарів у категорії {ComponentType}", productLinks.Count, componentType);

            var failedLinks = new List<string>(productLinks);
            var storesByName = new Dictionary<string, Store>();

            int outerRetry = 0;
            const int maxOuterRetries = 3;

            while (failedLinks.Any() && outerRetry < maxOuterRetries)
            {
                outerRetry++;
                Console.WriteLine($"\nЦикл обробки {outerRetry}/{maxOuterRetries}: {failedLinks.Count} посилань...");
                var successfulProxies = new ConcurrentDictionary<string, bool>();
                var failedProxies = new ConcurrentDictionary<string, bool>();

                if (_proxyPool.NeedsRefresh())
                {
                    Console.WriteLine("Завантаження та валідація нових проксі...");
                    var rawProxies = await _proxyScraper.GetProxiesAsync(cancellationToken);

                    if (rawProxies.Count == 0)
                    {
                        Console.WriteLine("Не вдалося завантажити жодного проксі. Завершення.");
                        return;
                    }

                    await _proxyPool.LoadAndValidateAsync(rawProxies, cancellationToken);

                    if (_proxyPool.AvailableCount == 0)
                    {
                        Console.WriteLine("Жоден проксі не пройшов валідацію. Завершення.");
                        return;
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
                        int maxRetries = 5;
                        int attempt = 0;

                        while (attempt < maxRetries)
                        {
                            string? currentProxy = null;
                            try
                            {
                                var proxyResult = CreateHttpClientWithProxy();
                                if (proxyResult == null)
                                {
                                    Console.WriteLine($"[{index}] Немає доступних проксі, пропуск спроби.");
                                    attempt++;
                                    await Task.Delay(Random.Shared.Next(1000, 2000) * (attempt + 1));
                                    continue;
                                }
                                var (client, proxy) = proxyResult.Value;
                                currentProxy = proxy;
                                using (client)
                                {
                                    var result = await scraper.ScrapeAsync(link, client, concurrentComponents, concurrentStores, cancellationToken);

                                    if (result.Component != null)
                                    {
                                        Console.WriteLine($"[{index}] Отримано компонент: {result.Component}");
                                        Console.WriteLine($"  Посилання: {link}");
                                        Console.WriteLine($"  Магазинів: {result.Stores.Count}, Пропозицій: {result.Offers.Count}");

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

                                    Console.WriteLine($"[{index}] Парсинг не вдався для {link}");
                                }
                            }
                            catch (Exception ex)
                            {
                                if (currentProxy != null)
                                    _proxyPool.ReturnProxy(currentProxy, success: false);
                                Console.WriteLine($"[{index}] Спроба {attempt + 1}/{maxRetries} не вдалася: {ex.Message}");
                            }

                            attempt++;
                            await Task.Delay(Random.Shared.Next(1000, 2000) * (attempt + 1));
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

                try
                {
                    Console.WriteLine("Збереження компонентів в базу даних...");

                    try
                    {
                        await TranslateDescriptionsAsync(componentsToSave, componentsFromDb, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Помилка перекладу описів");
                    }

                    int savedComponentsCount = 0;
                    int updatedComponentsCount = 0;
                    foreach (var component in componentsToSave)
                    {
                        var nameProperty = typeof(T).GetProperty("Name");
                        var componentName = nameProperty?.GetValue(component) as string;

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
                                            if (elementType == null) return;

                                            foreach (var oldItem in existingCollection.Cast<object>().ToList())
                                            {
                                                _context.Remove(oldItem);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Помилка при видаленні старих елементів: {ex.Message}");
                                        }

                                        var targetList = property.GetValue(existingComponent) as System.Collections.IList;
                                        targetList?.Clear();

                                        foreach (var newItem in newCollection)
                                        {
                                            targetList?.Add(newItem);
                                        }
                                    }
                                }
                                else if (property.PropertyType == typeof(LocalizedDescription)
                                    && property.GetValue(existingComponent) is LocalizedDescription existingLocalized
                                    && newValue is LocalizedDescription newLocalized)
                                {
                                    existingLocalized.Uk = newLocalized.Uk;
                                    existingLocalized.En = newLocalized.En;
                                }
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
                    foreach (var offer in offersToSave)
                    {
                        var existingOffer = await _context.Set<ProductOffer>()
                            .FirstOrDefaultAsync(o =>
                                o.ComponentId == offer.ComponentId &&
                                o.StoreId == offer.StoreId &&
                                o.ProductOfferUrl == offer.ProductOfferUrl, cancellationToken);

                        if (existingOffer != null)
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
                    Console.WriteLine($"Збережено: {savedComponentsCount} компонентів, {savedStoresCount} магазинів, {savedOffersCount} пропозицій");
                    Console.WriteLine($"Оновлено: {updatedComponentsCount} компонентів, {updatedStoresCount} магазинів, {updatedOffersCount} пропозицій");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка при збереженні даних");
                }

                failedLinks = linksToRetry?.ToList() ?? new List<string>();

                _logger.LogInformation("Цикл {Cycle}/{MaxCycles}: проксі {Available} доступних, {Success} успішних, {Failed} невдалих. Залишилось {Remaining} посилань.",
                    outerRetry, maxOuterRetries, _proxyPool.AvailableCount, successfulProxies.Count, failedProxies.Count, failedLinks.Count);
            }

            totalStopwatch.Stop();
            int totalSuccessful = productLinks.Count - failedLinks.Count;
            double successRate = productLinks.Count > 0 ? (double)totalSuccessful / productLinks.Count * 100 : 0;
            _logger.LogInformation("Скрапінг {ComponentType} завершено за {Duration:F1}с. Успішно: {Successful}/{Total} ({SuccessRate:F1}%)",
                componentType, totalStopwatch.Elapsed.TotalSeconds, totalSuccessful, productLinks.Count, successRate);
        }

        public async Task ScrapeSingleComponentAsync<T>(string componentUrl, ComponentType componentType, CancellationToken cancellationToken = default) where T : class
        {
            Console.WriteLine("Початок роботи ScrapeCategoryAsync\n");

            var scraper = _scraperFactory.GetScraper<T>();
            if (scraper == null)
            {
                Console.WriteLine("Скрейпер для цього типу не знайдено!");
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
                Console.WriteLine($"  Отримано компонент: {result.Component}");
                Console.WriteLine($"  Посилання: {componentUrl}");
                Console.WriteLine($"  Магазинів: {result.Stores.Count}, Пропозицій: {result.Offers.Count}");

                try
                {
                    await TranslateDescriptionsAsync([result.Component], [], cancellationToken);
                    Console.WriteLine("  Переклад виконано успішно.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Помилка перекладу: {ex.Message}");
                }

                foreach (var prop in result.Component.GetType().GetProperties())
                {
                    var value = prop.GetValue(result.Component);
                    if(prop.Name == "Description" && value is LocalizedDescription localized)
                    {
                        // Тепер у нас є змінна 'localized' типу LocalizedDescription
                        Console.WriteLine($"  {prop.Name} (UK): {localized.Uk}");
                        Console.WriteLine($"  {prop.Name} (EN): {localized.En}");
                    }
                    else
                    {
                        Console.WriteLine($"  {prop.Name}: {value}");
                    }
                }
                Console.WriteLine("\n");
            }
            else
            {
                Console.WriteLine($"  Не вдалось отримати компонент.");
            }
        }
    }
}
