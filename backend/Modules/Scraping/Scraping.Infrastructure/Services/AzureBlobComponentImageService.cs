using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scraping.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace Scraping.Infrastructure.Services
{
    public class AzureBlobComponentImageService : IComponentImageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AzureBlobComponentImageService> _logger;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private bool _containerInitialized;

        public AzureBlobComponentImageService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AzureBlobComponentImageService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            var connectionString = configuration["AzureStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
            var containerName = configuration["AzureStorage:ContainerName"] ?? "component-images";
            _containerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<string?> UploadComponentImageAsync(
            string imageUrl,
            string componentType,
            Guid componentId,
            CancellationToken cancellationToken = default)
        {
            var blobName = $"{componentType.ToLower()}/{componentId}.webp";
            return await UploadAsWebpAsync(imageUrl, blobName, $"{componentType}/{componentId}", cancellationToken);
        }

        public async Task<string?> UploadStoreLogoAsync(
            string logoUrl,
            Guid storeId,
            CancellationToken cancellationToken = default)
        {
            var blobName = $"store-logos/{storeId}.webp";
            return await UploadAsWebpAsync(logoUrl, blobName, $"store/{storeId}", cancellationToken);
        }

        private async Task<string?> UploadAsWebpAsync(
            string sourceUrl,
            string blobName,
            string logLabel,
            CancellationToken cancellationToken)
        {
            try
            {
                await EnsureContainerExistsAsync(cancellationToken);

                var blobClient = _containerClient.GetBlobClient(blobName);

                if (await blobClient.ExistsAsync(cancellationToken))
                    return blobClient.Uri.ToString();

                using var httpClient = _httpClientFactory.CreateClient("ImageDownloader");
                using var response = await httpClient.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to download image for {Label} from {Url}: {StatusCode}",
                        logLabel, sourceUrl, response.StatusCode);
                    return null;
                }

                using var downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var image = await Image.LoadAsync(downloadStream, cancellationToken);
                using var webpStream = new MemoryStream();
                await image.SaveAsync(webpStream, new WebpEncoder(), cancellationToken);
                webpStream.Position = 0;

                await blobClient.UploadAsync(webpStream, new BlobHttpHeaders { ContentType = "image/webp" }, cancellationToken: cancellationToken);

                _logger.LogInformation("Uploaded {Label} to blob storage as WebP", logLabel);
                return blobClient.Uri.ToString();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upload image for {Label} from {Url}", logLabel, sourceUrl);
                return null;
            }
        }

        private async Task EnsureContainerExistsAsync(CancellationToken cancellationToken)
        {
            if (_containerInitialized) return;

            await _initLock.WaitAsync(cancellationToken);
            try
            {
                if (!_containerInitialized)
                {
                    await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
                    _containerInitialized = true;
                }
            }
            finally
            {
                _initLock.Release();
            }
        }

        internal static bool IsBlobUrl(string? url) =>
            !string.IsNullOrEmpty(url) &&
            (url.Contains(".blob.core.windows.net/") ||
             url.Contains("127.0.0.1:10000/") ||
             url.Contains("localhost:10000/"));
    }
}
