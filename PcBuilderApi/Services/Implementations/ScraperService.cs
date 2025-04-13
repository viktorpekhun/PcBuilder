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

        //public async Task ScrapeAndSaveCategoryAsync<T>(string categoryUrl) where T : class
        //{
        //    var scraper = _scraperFactory.GetScraper<T>();
        //    if (scraper == null) return;

        //    var productLinks = await _paginationScraper.GetComponentLinksAsync(categoryUrl);

        //    foreach (var link in productLinks)
        //    {
        //        var component = await scraper.ScrapeAsync(link);
        //        if (component != null)
        //        {
        //            await _unitOfWork.Repository<T>().AddAsync(component);
        //        }
        //    }

        //    await _unitOfWork.SaveAsync();
        //}

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

        public async Task TestScrapeCategoryAsync<T>(string categoryUrl) where T : class
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
                                var component = await scraper.ScrapeAsync(link, client);

                                if (component != null)
                                {
                                    Console.WriteLine($"✅ [{index}] Отримано компонент: {component}");
                                    Console.WriteLine($"  Посилання: {link}");
                                    foreach (var prop in component.GetType().GetProperties())
                                    {
                                        var value = prop.GetValue(component);
                                        Console.Write($"  {prop.Name}: {value}");
                                    }
                                    Console.WriteLine("\n\n");

                                    componentsToSave.Add(component);

                                    lock (_lock)
                                    {
                                        if (!successfulProxies.Contains(proxy))
                                            successfulProxies.Add(proxy);
                                    }

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

                foreach (var component in componentsToSave)
                {
                    await _unitOfWork.Repository<T>().AddAsync(component);
                }
                await _unitOfWork.SaveAsync();

                failedLinks = linksToRetry?.ToList() ?? new List<string>();

                if (successfulProxies.Any())
                {
                    Console.WriteLine("🔄 Перехід до використання тільки успішних проксі...");
                    _proxies = successfulProxies.Distinct().ToList();
                    _proxyIndex = 0;
                }
                else
                {
                    // Якщо немає успішних — скидаємо список
                    _proxies = new List<string>();
                }

                Console.WriteLine("✅ Цикл обробки завершено.");
            }

            Console.WriteLine("🏁 Усі посилання успішно оброблені.");
        }

        //public async Task TestScrapeCategoryAsync<T>(string categoryUrl) where T : class
        //{
        //    Console.WriteLine("📦 Початок роботи TestScrapeCategoryAsync\n");

        //    var scraper = _scraperFactory.GetScraper<T>();
        //    if (scraper == null)
        //    {
        //        Console.WriteLine("❌ Скрейпер для цього типу не знайдено!");
        //        return;
        //    }

        //    var productLinks = await _paginationScraper.GetComponentLinksAsync(categoryUrl);
        //    Console.WriteLine($"🔎 Знайдено {productLinks.Count} товарів у категорії.");

        //    var failedLinks = new List<string>(productLinks);
        //    var successfulProxies = new List<string>();

        //    while (failedLinks.Any())
        //    {
        //        Console.WriteLine($"\n🔁 Новий цикл обробки {failedLinks.Count} посилань...");

        //        if (_proxies.Count == 0)
        //        {
        //            Console.WriteLine("📥 Завантаження нових проксі...");
        //            _proxies = await _proxyScraper.GetProxiesAsync();
        //            _proxyIndex = 0;

        //            if (_proxies.Count == 0)
        //            {
        //                Console.WriteLine("❌ Не вдалося завантажити жодного проксі. Завершення.");
        //                return;
        //            }
        //        }

        //        var componentsToSave = new ConcurrentBag<T>();
        //        var linksToRetry = new ConcurrentBag<string>();

        //        var tasks = failedLinks.Select(async (link, index) =>
        //        {
        //            int maxRetries = 3;
        //            int attempt = 0;
        //            var rnd = new Random();

        //            while (attempt < maxRetries)
        //            {
        //                try
        //                {
        //                    var (client, proxy) = CreateHttpClientWithProxy();
        //                    using (client)
        //                    {
        //                        var component = await scraper.ScrapeAsync(link, client);

        //                        if (component != null)
        //                        {
        //                            Console.WriteLine($"✅ [{index}] Отримано компонент: {component}");
        //                            Console.WriteLine($"  Посилання: {link}");
        //                            foreach (var prop in component.GetType().GetProperties())
        //                            {
        //                                var value = prop.GetValue(component);
        //                                Console.Write($"  {prop.Name}: {value}");
        //                            }
        //                            Console.WriteLine("\n\n");

        //                            componentsToSave.Add(component);

        //                            lock (_lock)
        //                            {
        //                                if (!successfulProxies.Contains(proxy))
        //                                    successfulProxies.Add(proxy);
        //                            }

        //                            return;
        //                        }

        //                        Console.WriteLine($"⚠️ [{index}] Парсинг не вдався для {link}");
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"❌ [{index}] Спроба {attempt + 1}/{maxRetries} не вдалася: {ex.Message}");
        //                }

        //                attempt++;
        //                await Task.Delay(rnd.Next(1500, 3000));
        //            }

        //            linksToRetry.Add(link);
        //        });

        //        await Task.WhenAll(tasks);

        //        foreach (var component in componentsToSave)
        //        {

        //            foreach (var prop in component.GetType().GetProperties())
        //            {
        //                var value = prop.GetValue(component);
        //                Console.Write($"  {prop.Name}: {value}");
        //            }
        //            Console.WriteLine("\n\n");
        //            await _unitOfWork.Repository<T>().AddAsync(component);
        //        }
        //        await _unitOfWork.SaveAsync();


        //        failedLinks = linksToRetry.ToList();

        //        if (successfulProxies.Any())
        //        {
        //            Console.WriteLine("🔄 Перехід до використання тільки успішних проксі...");
        //            _proxies = successfulProxies.Distinct().ToList();
        //            _proxyIndex = 0;
        //        }
        //        else
        //        {
        //            // Якщо немає успішних — скидаємо список
        //            _proxies = new List<string>();
        //        }

        //        Console.WriteLine("✅ Цикл обробки завершено.");
        //    }

        //    Console.WriteLine("🏁 Усі посилання успішно оброблені.");
        //    //await _unitOfWork.SaveAsync();
        //}

        //public async Task TestScrapeCategoryAsync<T>(string categoryUrl) where T : class
        //{
        //    Console.WriteLine("📦 Початок роботи TestScrapeCategoryAsync (без проксі, послідовно)\n");

        //    var scraper = _scraperFactory.GetScraper<T>();
        //    if (scraper == null)
        //    {
        //        Console.WriteLine("❌ Скрейпер для цього типу не знайдено!");
        //        return;
        //    }

        //    var productLinks = await _paginationScraper.GetComponentLinksAsync(categoryUrl);
        //    Console.WriteLine($"🔎 Знайдено {productLinks.Count} товарів у категорії.\n");

        //    var client = new HttpClient
        //    {
        //        Timeout = TimeSpan.FromSeconds(20)
        //    };

        //    var componentsToSave = new List<T>();

        //    int index = 0;
        //    foreach (var link in productLinks)
        //    {
        //        try
        //        {
        //            Console.WriteLine($"➡️ [{index}] Обробка посилання: {link}");

        //            var component = await scraper.ScrapeAsync(link, client);

        //            if (component != null)
        //            {
        //                Console.WriteLine($"✅ Отримано компонент: {component}");
        //                foreach (var prop in component.GetType().GetProperties())
        //                {
        //                    var value = prop.GetValue(component);
        //                    Console.WriteLine($"  {prop.Name}: {value}");
        //                }

        //                componentsToSave.Add(component);
        //            }
        //            else
        //            {
        //                Console.WriteLine($"⚠️ Компонент не отримано: {link}");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"❌ Помилка при обробці [{index}]: {ex.Message}");
        //        }

        //        index++;
        //        await Task.Delay(1000); // невелика затримка, щоб не навантажувати сайт
        //    }

        //    Console.WriteLine("\n🏁 Обробку завершено.");

        //    //foreach (var component in componentsToSave)
        //    //{
        //    //    await _unitOfWork.Repository<T>().AddAsync(component);
        //    //}
        //    //await _unitOfWork.SaveAsync();
        //}





        //public async Task<bool> ScrapeAndSaveComponentAsync<T>(string url) where T : class
        //{
        //    var scraper = _scraperFactory.GetScraper<T>();
        //    if (scraper == null) return false;

        //    var component = await scraper.ScrapeAsync(url);
        //    if (component == null) return false;

        //    await _unitOfWork.Repository<T>().AddAsync(component);
        //    await _unitOfWork.SaveAsync();
        //    return true;
        //}
    }
}
