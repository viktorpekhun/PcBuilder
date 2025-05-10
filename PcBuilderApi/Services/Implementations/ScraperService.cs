
using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Scrapers;
using System.Collections.Concurrent;
using System.Net;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Implementations
{
    public class ScraperService
    {
        private readonly ComponentScraperFactory _scraperFactory;
        private readonly IPaginationScraper _paginationScraper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProxyScraper _proxyScraper;

        private List<string> _proxies = new();
        private int _proxyIndex = 0;
        private readonly object _lock = new();

        public ScraperService(ComponentScraperFactory scraperFactory, IPaginationScraper paginationScraper, IUnitOfWork unitOfWork, IProxyScraper proxyScraper)
        {
            _scraperFactory = scraperFactory;
            _paginationScraper = paginationScraper;
            _unitOfWork = unitOfWork;
            _proxyScraper = proxyScraper;
        }

        private (HttpClient client, string proxy) CreateHttpClientWithProxy()
        {
            string proxy;
            lock (_lock)
            {
                _proxyIndex = new Random().Next(_proxies.Count);
                proxy = _proxies[_proxyIndex];
            }

            var proxyUri = new WebProxy(proxy, false);
            var handler = new HttpClientHandler
            {
                Proxy = proxyUri,
                UseProxy = true,
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            var client = new HttpClient(handler, disposeHandler: true)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Referrer = new Uri("https://hotline.ua/");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            return (client, proxy);
        }
        private string GetIncludePropertiesForCollections<T>()
        {
            var includeProperties = new List<string>();

            foreach (var property in typeof(T).GetProperties())
            {
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                {
                    includeProperties.Add(property.Name);
                }
            }

            return string.Join(",", includeProperties);
        }

        public async Task ScrapeCategoryAsync<T>(string categoryUrl, ComponentType componentType) where T : class
        {
            Console.WriteLine("📦 Початок роботи TestScrapeCategoryAsync\n");

            var scraper = _scraperFactory.GetScraper<T>();
            if (scraper == null)
            {
                Console.WriteLine("❌ Скрейпер для цього типу не знайдено!");
                return;
            }

            var productLinks = await _paginationScraper.GetComponentLinksAsync(categoryUrl);
            Console.WriteLine($"🔎 Знайдено {productLinks.Count} товарів у категорії.");

            var failedLinks = new List<string>(productLinks);
            var successfulProxies = new List<string>();
            var failedProxies = new List<string>();

            var storesByName = new Dictionary<string, Store>();

            while (failedLinks.Any())
            {
                Console.WriteLine($"\n🔁 Новий цикл обробки {failedLinks.Count} посилань...");
                successfulProxies = new List<string>();
                failedProxies = new List<string>();

                if (_proxies.Count == 0)
                {
                    Console.WriteLine("📥 Завантаження нових проксі...");
                    _proxies = await _proxyScraper.GetProxiesAsync();
                    _proxyIndex = 0;

                    if (_proxies.Count == 0)
                    {
                        Console.WriteLine("❌ Не вдалося завантажити жодного проксі. Завершення.");
                        return;
                    }
                }

                var componentsToSave = new ConcurrentBag<T>();
                var storesToSave = new ConcurrentBag<Store>();
                var offersToSave = new ConcurrentBag<ProductOffer>();
                var linksToRetry = new ConcurrentBag<string>();
                var includeProperties = GetIncludePropertiesForCollections<T>();
                var componentsFromDb = await _unitOfWork.Repository<T>().GetAllAsync(includeProperties);
                ConcurrentBag<T> concurrentComponents = new ConcurrentBag<T>(componentsFromDb);
                var existingStoresFromDb = await _unitOfWork.Repository<Store>().GetAllAsync();
                ConcurrentBag<Store> concurrentStores = new ConcurrentBag<Store>(existingStoresFromDb);

                var tasks = failedLinks.Select(async (link, index) =>
                {
                    int maxRetries = 3;
                    int attempt = 0;
                    var rnd = new Random();

                    while (attempt < maxRetries)
                    {
                        try
                        {
                            var (client, proxy) = CreateHttpClientWithProxy();
                            using (client)
                            {
                                var result = await scraper.ScrapeAsync(link, client, concurrentComponents, concurrentStores);

                                if (result.Component != null)
                                {
                                    Console.WriteLine($"✅ [{index}] Отримано компонент: {result.Component}");
                                    Console.WriteLine($"  Посилання: {link}");
                                    Console.WriteLine($"  Магазинів: {result.Stores.Count}, Пропозицій: {result.Offers.Count}");

                                    // Log component properties
                                    foreach (var prop in result.Component.GetType().GetProperties())
                                    {
                                        var value = prop.GetValue(result.Component);
                                        Console.Write($"  {prop.Name}: {value}");
                                    }
                                    Console.WriteLine("\n");

                                    componentsToSave.Add(result.Component);

                                    // Process stores and offers with thread safety
                                    lock (_lock)
                                    {
                                        foreach (var store in result.Stores)
                                        {
                                            // Check if store already exists in our dictionary by name
                                            if (!storesByName.TryGetValue(store.Name, out var existingStore))
                                            {
                                                // New store - add to dictionary
                                                storesByName[store.Name] = store;
                                                storesToSave.Add(store);
                                            }
                                            else
                                            {
                                                // Update offers to use existing store ID
                                                foreach (var offer in result.Offers.Where(o => o.StoreId == store.Id))
                                                {
                                                    offer.StoreId = existingStore.Id;
                                                }
                                            }
                                        }

                                        // Add all offers
                                        foreach (var offer in result.Offers)
                                        {
                                            offersToSave.Add(offer);
                                        }
                                    }

                                    if (!successfulProxies.Contains(proxy))
                                        successfulProxies.Add(proxy);

                                    return;
                                }
                                else
                                {
                                    if (!failedProxies.Contains(proxy))
                                        failedProxies.Add(proxy);
                                }

                                Console.WriteLine($"⚠️ [{index}] Парсинг не вдався для {link}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ [{index}] Спроба {attempt + 1}/{maxRetries} не вдалася: {ex.Message}");
                        }

                        attempt++;
                        await Task.Delay(rnd.Next(1500, 3000));
                    }

                    linksToRetry.Add(link);
                });

                await Task.WhenAll(tasks);

                try
                {
                    Console.WriteLine("💾 Збереження компонентів в базу даних...");
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
                                                var deleteMethod = _unitOfWork.GetType().GetMethod("Repository")
                                                    ?.MakeGenericMethod(elementType)
                                                    .Invoke(_unitOfWork, null);

                                                var getByIdMethod = deleteMethod?.GetType().GetMethod("GetByIdAsync");
                                                var idProperty = oldItem.GetType().GetProperty("Id");
                                                var idValue = idProperty?.GetValue(oldItem);

                                                var existing = getByIdMethod?.Invoke(deleteMethod, new object[] { idValue });
                                                if (existing == null) continue;

                                                var deleteAsyncMethod = deleteMethod?.GetType().GetMethod("DeleteAsync");
                                                deleteAsyncMethod?.Invoke(deleteMethod, new[] { oldItem });
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"❌ Помилка при видаленні старих елементів: {ex.Message}");
                                        }

                                        var targetList = property.GetValue(existingComponent) as System.Collections.IList;
                                        targetList?.Clear();

                                        foreach (var newItem in newCollection)
                                        {
                                            targetList?.Add(newItem);
                                        }
                                    }
                                }
                                else
                                {
                                    property.SetValue(existingComponent, newValue);
                                }
                            }

                            await _unitOfWork.Repository<T>().UpdateAsync(existingComponent);
                            updatedComponentsCount++;
                        }
                        else
                        {
                            await _unitOfWork.Repository<T>().AddAsync(component);
                            savedComponentsCount++;
                        }
                    }

                    int savedStoresCount = 0;
                    int updatedStoresCount = 0;
                    foreach (var store in storesToSave)
                    {
                        var existingStore = await _unitOfWork.Repository<Store>().GetFirstOrDefaultAsync(s => s.Id == store.Id);
                        if (existingStore != null)
                        {
                            existingStore.Name = store.Name;
                            existingStore.LogoUrl = store.LogoUrl;
                            existingStore.Likes = store.Likes;
                            existingStore.Dislikes = store.Dislikes;
                            await _unitOfWork.Repository<Store>().UpdateAsync(existingStore);
                            updatedStoresCount++;
                        }
                        else
                        {
                            await _unitOfWork.Repository<Store>().AddAsync(store);
                            savedStoresCount++;
                        }
                    }

                    int savedOffersCount = 0;
                    int updatedOffersCount = 0;
                    foreach (var offer in offersToSave)
                    {
                        var existingOffer = await _unitOfWork.Repository<ProductOffer>()
                            .GetFirstOrDefaultAsync(o =>
                                o.ComponentId == offer.ComponentId &&
                                o.StoreId == offer.StoreId &&
                                o.ProductOfferUrl == offer.ProductOfferUrl);

                        if (existingOffer != null)
                        {
                            existingOffer.Price = offer.Price;
                            await _unitOfWork.Repository<ProductOffer>().UpdateAsync(existingOffer);
                            updatedOffersCount++;
                        }
                        else
                        {
                            await _unitOfWork.Repository<ProductOffer>().AddAsync(offer);
                            savedOffersCount++;
                        }
                    }

                    await _unitOfWork.SaveAsync();
                    Console.WriteLine($"✅ Збережено: {savedComponentsCount} компонентів, {savedStoresCount} магазинів, {savedOffersCount} пропозицій");
                    Console.WriteLine($"✅ Оновлено: {updatedComponentsCount} компонентів, {updatedStoresCount} магазинів, {updatedOffersCount} пропозицій");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка при збереженні даних: {ex.Message}");
                }

                failedLinks = linksToRetry?.ToList() ?? new List<string>();

                if (successfulProxies.Any() && successfulProxies.Count > 1)
                {
                    Console.WriteLine("🔄 Перехід до використання тільки успішних проксі...");
                    _proxies = successfulProxies.Distinct().ToList();
                    _proxyIndex = 0;
                }
                else
                {
                    if (_proxies.Count > 3)
                    {
                        _proxies.RemoveAll(proxy => failedProxies.Contains(proxy));
                    }
                    else
                    {
                        _proxies = new List<string>();
                    }
                }

                Console.WriteLine("✅ Цикл обробки завершено.");
            }

            Console.WriteLine("🏁 Усі посилання успішно оброблені.");
        }
    }
}