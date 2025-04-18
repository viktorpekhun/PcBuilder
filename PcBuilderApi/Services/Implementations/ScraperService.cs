//using PcBuilderApi.Repositories.Interfaces;
//using PcBuilderApi.Scrapers;
//using System.Collections.Concurrent;
//using System.Net;

//namespace PcBuilderApi.Services.Implementations
//{
//    public class ScraperService
//    {
//        private readonly ComponentScraperFactory _scraperFactory;
//        private readonly IPaginationScraper _paginationScraper;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IProxyScraper _proxyScraper;

//        private List<string> _proxies = new();
//        private int _proxyIndex = 0;
//        private readonly object _lock = new();

//        public ScraperService(ComponentScraperFactory scraperFactory, IPaginationScraper paginationScraper, IUnitOfWork unitOfWork, IProxyScraper proxyScraper)
//        {
//            _scraperFactory = scraperFactory;
//            _paginationScraper = paginationScraper;
//            _unitOfWork = unitOfWork;
//            _proxyScraper = proxyScraper;
//        }

//        private (HttpClient client, string proxy) CreateHttpClientWithProxy()
//        {
//            string proxy;
//            lock (_lock)
//            {
//                proxy = _proxies[_proxyIndex];
//                _proxyIndex = (_proxyIndex + 1) % _proxies.Count;
//            }

//            var proxyUri = new WebProxy(proxy, false);
//            var handler = new HttpClientHandler
//            {
//                Proxy = proxyUri,
//                UseProxy = true,
//                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
//            };

//            var client = new HttpClient(handler, disposeHandler: true)
//            {
//                Timeout = TimeSpan.FromSeconds(15)
//            };

//            // Додаємо заголовки
//            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
//            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");
//            client.DefaultRequestHeaders.Referrer = new Uri("https://hotline.ua/");
//            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

//            return (client, proxy);
//        }

//        public async Task TestScrapeCategoryAsync<T>(string categoryUrl) where T : class
//        {
//            Console.WriteLine("📦 Початок роботи TestScrapeCategoryAsync\n");

//            var scraper = _scraperFactory.GetScraper<T>();
//            if (scraper == null)
//            {
//                Console.WriteLine("❌ Скрейпер для цього типу не знайдено!");
//                return;
//            }

//            var productLinks = await _paginationScraper.GetComponentLinksAsync(categoryUrl);
//            Console.WriteLine($"🔎 Знайдено {productLinks.Count} товарів у категорії.");

//            var failedLinks = new List<string>(productLinks);
//            var successfulProxies = new List<string>();

//            while (failedLinks.Any())
//            {
//                Console.WriteLine($"\n🔁 Новий цикл обробки {failedLinks.Count} посилань...");
//                successfulProxies = new List<string>();

//                if (_proxies.Count == 0)
//                {
//                    Console.WriteLine("📥 Завантаження нових проксі...");
//                    _proxies = await _proxyScraper.GetProxiesAsync();
//                    _proxyIndex = 0;

//                    if (_proxies.Count == 0)
//                    {
//                        Console.WriteLine("❌ Не вдалося завантажити жодного проксі. Завершення.");
//                        return;
//                    }
//                }

//                var componentsToSave = new ConcurrentBag<T>();
//                var linksToRetry = new ConcurrentBag<string>();

//                var tasks = failedLinks.Select(async (link, index) =>
//                {
//                    int maxRetries = 3;
//                    int attempt = 0;
//                    var rnd = new Random();

//                    while (attempt < maxRetries)
//                    {
//                        try
//                        {
//                            var (client, proxy) = CreateHttpClientWithProxy();
//                            using (client)
//                            {
//                                var component = await scraper.ScrapeAsync(link, client);

//                                if (component != null)
//                                {
//                                    Console.WriteLine($"✅ [{index}] Отримано компонент: {component}");
//                                    Console.WriteLine($"  Посилання: {link}");
//                                    foreach (var prop in component.GetType().GetProperties())
//                                    {
//                                        var value = prop.GetValue(component);
//                                        Console.Write($"  {prop.Name}: {value}");
//                                    }
//                                    Console.WriteLine("\n\n");

//                                    componentsToSave.Add(component);

//                                    lock (_lock)
//                                    {
//                                        if (!successfulProxies.Contains(proxy))
//                                            successfulProxies.Add(proxy);
//                                    }

//                                    return;
//                                }

//                                Console.WriteLine($"⚠️ [{index}] Парсинг не вдався для {link}");
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            Console.WriteLine($"❌ [{index}] Спроба {attempt + 1}/{maxRetries} не вдалася: {ex.Message}");
//                        }

//                        attempt++;
//                        await Task.Delay(rnd.Next(1500, 3000));
//                    }

//                    linksToRetry.Add(link);
//                });

//                await Task.WhenAll(tasks);

//                foreach (var component in componentsToSave)
//                {
//                    await _unitOfWork.Repository<T>().AddAsync(component);
//                }
//                await _unitOfWork.SaveAsync();

//                failedLinks = linksToRetry?.ToList() ?? new List<string>();

//                if (successfulProxies.Any())
//                {
//                    Console.WriteLine("🔄 Перехід до використання тільки успішних проксі...");
//                    _proxies = successfulProxies.Distinct().ToList();
//                    _proxyIndex = 0;
//                }
//                else
//                {
//                    _proxies = new List<string>();
//                }

//                Console.WriteLine("✅ Цикл обробки завершено.");
//            }

//            Console.WriteLine("🏁 Усі посилання успішно оброблені.");
//        }


//    }
//}


using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Scrapers;
using System.Collections.Concurrent;
using System.Net;

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
                proxy = _proxies[_proxyIndex];
                _proxyIndex = (_proxyIndex + 1) % _proxies.Count;
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

            // Додаємо заголовки
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Referrer = new Uri("https://hotline.ua/");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            return (client, proxy);
        }

        public async Task TestScrapeCategoryAsync<T>(string categoryUrl, Utilities.SD.ComponentType componentType) where T : class
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

            // Dictionary to track stores by name for deduplication
            var storesByName = new Dictionary<string, Store>();

            while (failedLinks.Any())
            {
                Console.WriteLine($"\n🔁 Новий цикл обробки {failedLinks.Count} посилань...");
                successfulProxies = new List<string>();

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
                                var result = await scraper.ScrapeAsync(link, client);

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

                // Now save everything to the database
                try
                {
                    Console.WriteLine("💾 Збереження компонентів в базу даних...");

                    // Save components
                    foreach (var component in componentsToSave)
                    {
                        await _unitOfWork.Repository<T>().AddAsync(component);
                    }

                    // Save stores (only new ones)
                    foreach (var store in storesToSave)
                    {
                        await _unitOfWork.Repository<Store>().AddAsync(store);
                    }

                    // Update component IDs in offers
                    foreach (var offer in offersToSave)
                    {
                        // Make sure the component ID is set
                        if (componentsToSave.Any())
                        {
                            // Find the matching component
                            var firstComponent = componentsToSave.First();
                            var idProperty = firstComponent.GetType().GetProperty("Id");
                            if (idProperty != null)
                            {
                                var componentId = (Guid)idProperty.GetValue(firstComponent);
                                offer.ComponentId = componentId;
                            }
                        }

                        offer.ComponentType = componentType;
                        await _unitOfWork.Repository<ProductOffer>().AddAsync(offer);
                    }

                    await _unitOfWork.SaveAsync();
                    Console.WriteLine($"✅ Збережено: {componentsToSave.Count} компонентів, {storesToSave.Count} магазинів, {offersToSave.Count} пропозицій");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Помилка при збереженні даних: {ex.Message}");
                }

                failedLinks = linksToRetry?.ToList() ?? new List<string>();

                if (successfulProxies.Any())
                {
                    Console.WriteLine("🔄 Перехід до використання тільки успішних проксі...");
                    _proxies = successfulProxies.Distinct().ToList();
                    _proxyIndex = 0;
                }
                else
                {
                    _proxies = new List<string>();
                }

                Console.WriteLine("✅ Цикл обробки завершено.");
            }

            Console.WriteLine("🏁 Усі посилання успішно оброблені.");
        }
    }
}