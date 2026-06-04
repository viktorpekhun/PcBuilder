using Auth.Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace Auth.Infrastructure.Services
{
    public class AzureBlobFileStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobFileStorageService> _logger;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private bool _containerInitialized;

        public AzureBlobFileStorageService(IConfiguration configuration, ILogger<AzureBlobFileStorageService> logger)
        {
            _logger = logger;

            var connectionString = configuration["AzureStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
            var containerName = configuration["AzureStorage:ContainerName"] ?? "component-images";
            _containerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<string> SaveAvatarAsync(Guid userId, byte[] data, CancellationToken cancellationToken = default)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobName = $"{userId}.webp";
            var blobClient = _containerClient.GetBlobClient(blobName);

            using var inputStream = new MemoryStream(data);
            using var image = await Image.LoadAsync(inputStream, cancellationToken);
            using var webpStream = new MemoryStream();
            await image.SaveAsync(webpStream, new WebpEncoder(), cancellationToken);
            webpStream.Position = 0;

            await blobClient.UploadAsync(webpStream, new BlobHttpHeaders { ContentType = "image/webp" }, cancellationToken: cancellationToken);

            _logger.LogInformation("Uploaded avatar for user {UserId} to blob storage", userId);
            return blobClient.Uri.ToString();
        }

        public async Task DeleteAvatarAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobClient = _containerClient.GetBlobClient($"{userId}.webp");
            var deleted = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            if (deleted)
                _logger.LogInformation("Deleted avatar for user {UserId} from blob storage", userId);
        }

        public async Task<string> SaveBuildPhotoAsync(Guid buildId, byte[] data, CancellationToken cancellationToken = default)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobName = $"builds/{buildId}.webp";
            var blobClient = _containerClient.GetBlobClient(blobName);

            using var inputStream = new MemoryStream(data);
            using var image = await Image.LoadAsync(inputStream, cancellationToken);
            using var webpStream = new MemoryStream();
            await image.SaveAsync(webpStream, new WebpEncoder(), cancellationToken);
            webpStream.Position = 0;

            await blobClient.UploadAsync(webpStream, new BlobHttpHeaders { ContentType = "image/webp" }, cancellationToken: cancellationToken);

            _logger.LogInformation("Uploaded photo for build {BuildId} to blob storage", buildId);
            return blobClient.Uri.ToString();
        }

        public async Task DeleteBuildPhotoAsync(Guid buildId, CancellationToken cancellationToken = default)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobClient = _containerClient.GetBlobClient($"builds/{buildId}.webp");
            var deleted = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            if (deleted)
                _logger.LogInformation("Deleted photo for build {BuildId} from blob storage", buildId);
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
    }
}
