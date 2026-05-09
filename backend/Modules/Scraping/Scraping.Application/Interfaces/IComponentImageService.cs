namespace Scraping.Application.Interfaces
{
    public interface IComponentImageService
    {
        Task<string?> UploadComponentImageAsync(
            string imageUrl,
            string componentType,
            Guid componentId,
            CancellationToken cancellationToken = default);

        Task<string?> UploadStoreLogoAsync(
            string logoUrl,
            Guid storeId,
            CancellationToken cancellationToken = default);
    }
}
