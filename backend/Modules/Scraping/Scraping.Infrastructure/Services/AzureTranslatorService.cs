using Microsoft.Extensions.Options;
using Scraping.Application.Interfaces;
using Scraping.Infrastructure.Configuration;
using System.Text;
using System.Text.Json;

namespace Scraping.Infrastructure.Services
{
    public class AzureTranslatorService : ITranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly AzureTranslatorOptions _options;

        public AzureTranslatorService(HttpClient httpClient, IOptions<AzureTranslatorOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> TranslateAsync(string text, string from, string to, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var results = await TranslateBatchAsync([text], from, to, cancellationToken);
            return results.FirstOrDefault() ?? string.Empty;
        }

        public async Task<IReadOnlyList<string>> TranslateBatchAsync(IReadOnlyList<string> texts, string from, string to, CancellationToken cancellationToken = default)
        {
            if (texts == null || texts.Count == 0)
                return [];

            var results = new List<string>();

            const int maxElements = 25;
            const int maxChars = 40000;
            const int maxRetries = 3;

            var chunks = new List<List<string>>();
            var currentChunk = new List<string>();
            int currentChunkChars = 0;

            foreach (var text in texts)
            {
                if (currentChunk.Count >= maxElements || (currentChunkChars + text.Length > maxChars && currentChunk.Count > 0))
                {
                    chunks.Add(currentChunk);
                    currentChunk = new List<string>();
                    currentChunkChars = 0;
                }
                currentChunk.Add(text);
                currentChunkChars += text.Length;
            }
            if (currentChunk.Count > 0)
                chunks.Add(currentChunk);

            foreach (var chunk in chunks)
            {
                bool chunkTranslated = false;

                for (int attempt = 0; attempt < maxRetries; attempt++)
                {
                    try
                    {
                        if (attempt > 0)
                            await Task.Delay(3000 * attempt, cancellationToken);

                        var requestBody = chunk.Select(t => new { Text = t }).ToArray();
                        var json = JsonSerializer.Serialize(requestBody);

                        var request = new HttpRequestMessage(HttpMethod.Post,
                            $"{_options.Endpoint.TrimEnd('/')}/translate?api-version=3.0&from={from}&to={to}");
                        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                        request.Headers.Add("Ocp-Apim-Subscription-Key", _options.Key);
                        request.Headers.Add("Ocp-Apim-Subscription-Region", _options.Region);

                        var response = await _httpClient.SendAsync(request, cancellationToken);
                        response.EnsureSuccessStatusCode();

                        var responseJson = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(responseJson);

                        foreach (var element in doc.RootElement.EnumerateArray())
                        {
                            var translations = element.GetProperty("translations");
                            var translatedText = translations[0].GetProperty("text").GetString() ?? string.Empty;
                            results.Add(translatedText);
                        }

                        chunkTranslated = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Translation chunk attempt {attempt + 1}/{maxRetries} failed: {ex.Message}");
                    }
                }

                if (!chunkTranslated)
                {
                    Console.WriteLine($"Translation chunk failed after {maxRetries} retries, skipping {chunk.Count} items.");
                    results.AddRange(chunk.Select(_ => string.Empty));
                }

                if (chunks.Count > 1)
                    await Task.Delay(5000, cancellationToken);
            }

            return results;
        }
    }
}
