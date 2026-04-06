using System.Collections.Concurrent;
using System.Net;

namespace Scraping.Infrastructure.Services
{
    public class ProxyPool
    {
        private readonly ConcurrentQueue<ProxyEntry> _available = new();
        private readonly ConcurrentDictionary<string, ProxyStats> _stats = new();
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        private const int MaxConsecutiveFailures = 3;
        private const int CooldownMs = 3000;
        private const int ValidationTimeoutMs = 5000;
        private const int ValidationConcurrency = 50;

        public int AvailableCount => _available.Count;

        public async Task LoadAndValidateAsync(List<string> rawProxies, CancellationToken cancellationToken = default)
        {
            if (!await _refreshLock.WaitAsync(0, cancellationToken))
                return;

            try
            {
                Console.WriteLine($"Валідація {rawProxies.Count} проксі...");

                var validated = new ConcurrentBag<string>();
                var throttle = new SemaphoreSlim(ValidationConcurrency, ValidationConcurrency);

                var tasks = rawProxies.Distinct().Select(async proxy =>
                {
                    await throttle.WaitAsync(cancellationToken);
                    try
                    {
                        if (await TestProxyAsync(proxy, cancellationToken))
                        {
                            validated.Add(proxy);
                        }
                    }
                    finally
                    {
                        throttle.Release();
                    }
                });

                await Task.WhenAll(tasks);

                while (_available.TryDequeue(out _)) { }
                _stats.Clear();

                foreach (var proxy in validated)
                {
                    _available.Enqueue(new ProxyEntry(proxy, DateTime.MinValue));
                    _stats.TryAdd(proxy, new ProxyStats());
                }

                Console.WriteLine($"Валідовано {validated.Count}/{rawProxies.Count} проксі.");
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private static async Task<bool> TestProxyAsync(string proxy, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var proxyAddress = proxy.Contains("://") ? proxy : $"http://{proxy}";
                var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxyAddress, false),
                    UseProxy = true,
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };

                using var client = new HttpClient(handler, disposeHandler: true)
                {
                    Timeout = TimeSpan.FromMilliseconds(ValidationTimeoutMs)
                };

                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

                var request = new HttpRequestMessage(HttpMethod.Head, "https://hotline.ua/");
                var response = await client.SendAsync(request, cancellationToken);
                return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Redirect
                    || response.StatusCode == System.Net.HttpStatusCode.MovedPermanently;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        public string? RentProxy()
        {
            int attempts = 0;
            int maxAttempts = _available.Count + 5;

            while (attempts < maxAttempts && _available.TryDequeue(out var entry))
            {
                attempts++;

                var elapsed = (DateTime.UtcNow - entry.LastUsed).TotalMilliseconds;
                if (elapsed < CooldownMs)
                {
                    _available.Enqueue(entry);
                    Thread.SpinWait(100);
                    continue;
                }

                if (_stats.TryGetValue(entry.Address, out var stats) && stats.ConsecutiveFailures >= MaxConsecutiveFailures)
                {
                    continue;
                }

                return entry.Address;
            }

            return null;
        }

        public void ReturnProxy(string proxy, bool success)
        {
            if (_stats.TryGetValue(proxy, out var stats))
            {
                if (success)
                {
                    stats.SuccessCount++;
                    stats.ConsecutiveFailures = 0;
                }
                else
                {
                    stats.FailureCount++;
                    stats.ConsecutiveFailures++;
                }
            }

            if (!success && _stats.TryGetValue(proxy, out var s) && s.ConsecutiveFailures >= MaxConsecutiveFailures)
            {
                return;
            }

            _available.Enqueue(new ProxyEntry(proxy, DateTime.UtcNow));
        }

        public bool NeedsRefresh(int minProxies = 5)
        {
            return _available.Count < minProxies;
        }

        private record ProxyEntry(string Address, DateTime LastUsed);

        private class ProxyStats
        {
            public int SuccessCount { get; set; }
            public int FailureCount { get; set; }
            public int ConsecutiveFailures { get; set; }
        }
    }
}
