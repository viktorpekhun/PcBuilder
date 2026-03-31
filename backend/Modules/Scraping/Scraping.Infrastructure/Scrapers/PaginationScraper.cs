using Scraping.Application.Interfaces;
﻿using HtmlAgilityPack;
using PcBuilder.SharedKernel;
using Scraping.Infrastructure.Utilities;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Scraping.Infrastructure.Scrapers
{


    public class HotlineGraphQLClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _path;
        private readonly int[] _filters;

        public HotlineGraphQLClient(string token, string categoryUrl)
        {

            (_path, _filters, var urlWithoutUa) = ParseUrl(categoryUrl);

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://hotline.ua");

            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://hotline.ua");
            _httpClient.DefaultRequestHeaders.Add("Referer", categoryUrl);
            _httpClient.DefaultRequestHeaders.Add("x-language", "uk");
            _httpClient.DefaultRequestHeaders.Add("x-referer", urlWithoutUa);
            _httpClient.DefaultRequestHeaders.Add("x-request-id", Guid.NewGuid().ToString());
            _httpClient.DefaultRequestHeaders.Add("x-token", token);

            _httpClient.DefaultRequestHeaders.Add("Cookie", "city_id=223; language=uk;");
        }

        public async Task<JToken> GetPageAsync(int page)
        {
            var variables = new Dictionary<string, object>
            {
                { "path", _path },
                { "cityId", 223 },
                { "sort", "popularity" },
                { "itemsPerPage", 48 },
                { "filters", _filters },
                { "excludedFilters", Array.Empty<int>() }
            };

            if (page != 0)
            {
                variables["page"] = page;
            }

            var query = new
            {
                operationName = "getCatalogProducts",
                variables,
                query = @"query getCatalogProducts($path: String!, $cityId: Int, $sort: String, $showFirst: String, $phrase: String, $itemsPerPage: Int, $page: Int, $filters: [Int], $excludedFilters: [Int], $priceMin: Int, $priceMax: Int) {
                  byPathSectionQueryProducts(path: $path, cityId: $cityId, sort: $sort, showFirst: $showFirst, phrase: $phrase, itemsPerPage: $itemsPerPage, page: $page, filters: $filters, excludedFilters: $excludedFilters, priceMin: $priceMin, priceMax: $priceMax) {
                    collection {
                      _id
                      title
                      url
                      minPrice
                      maxPrice
                      vendor { title }
                    }
                    paginationInfo {
                      lastPage
                      totalCount
                    }
                  }
                }"
            };

            var content = new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/svc/frontend-api/graphql", content);

            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            return JToken.Parse(jsonString);
        }

        public (string path, int[] filters, string urlWithoutUa) ParseUrl(string url)
        {
            var uri = new Uri(url);

            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            string path = segments.Length >= 3 ? segments[2] : string.Empty;

            int[] filters = Array.Empty<int>();
            if (segments.Length >= 4)
            {
                filters = segments[3]
                    .Split('-', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out var num) ? num : -1)
                    .Where(n => n != -1)
                    .ToArray();
            }

            var urlWithoutUa = url.Replace("/ua", "");

            return (path, filters, urlWithoutUa);
        }
    }



    public class PaginationScraper : IPaginationScraper
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://hotline.ua";    

        public PaginationScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<List<string>> GetComponentLinksAsync(string categoryUrl, CancellationToken cancellationToken = default)
        {
            var componentLinks = new List<string>();
            int currentPage = 0;
            string html = await _httpClient.GetStringAsync(categoryUrl, cancellationToken);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nuxtData = NuxtScriptWorker.ExtractNuxtDataFromHtml(doc);
            if (nuxtData != null)
            {
                var result = NuxtScriptWorker.FindTokenByKey(nuxtData, "state");
                var pageType = NuxtScriptWorker.FindTokenByKey(result, "pageType");
                var tokenData = NuxtScriptWorker.FindTokenByKey(pageType, "token");

                var client = new HotlineGraphQLClient(tokenData.ToString(), categoryUrl);

                int consecutiveFailures = 0;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    JToken? json = null;
                    const int maxPageRetries = 3;

                    for (int pageAttempt = 0; pageAttempt < maxPageRetries; pageAttempt++)
                    {
                        try
                        {
                            if (pageAttempt > 0)
                                await Task.Delay(2000 * pageAttempt, cancellationToken);

                            json = await client.GetPageAsync(currentPage);
                            consecutiveFailures = 0;
                            break;
                        }
                        catch (OperationCanceledException) { throw; }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Сторінка {currentPage}, спроба {pageAttempt + 1}/{maxPageRetries} не вдалася: {ex.Message}");
                        }
                    }

                    if (json == null)
                    {
                        consecutiveFailures++;
                        Console.WriteLine($"Пропуск сторінки {currentPage} після {maxPageRetries} спроб.");

                        if (consecutiveFailures >= 3)
                        {
                            Console.WriteLine("3 послідовні невдалі сторінки, завершення пагінації.");
                            break;
                        }

                        currentPage++;
                        await Task.Delay(2000, cancellationToken);
                        continue;
                    }

                    var products = json["data"]?["byPathSectionQueryProducts"]?["collection"];
                    if (products == null || !products.Any())
                    {
                        break;
                    }

                    foreach (var product in products)
                    {
                        var productUrl = product?["url"]?.ToString().Trim();
                        if (!string.IsNullOrEmpty(productUrl))
                        {
                            componentLinks.Add($"{BASE_URL}{productUrl}");
                        }
                    }
                    Console.WriteLine($"Fetched {products.Count()} products from page {currentPage}.");

                    currentPage++;
                    await Task.Delay(2000, cancellationToken);
                }

            }
            else
            {
                return new List<string>();
            }


            return componentLinks;
        }
    }



}
