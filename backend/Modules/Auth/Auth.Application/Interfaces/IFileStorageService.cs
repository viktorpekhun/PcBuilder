namespace Auth.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAvatarAsync(Guid userId, byte[] data, CancellationToken cancellationToken = default);
        Task DeleteAvatarAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<string> SaveBuildPhotoAsync(Guid buildId, byte[] data, CancellationToken cancellationToken = default);
        Task DeleteBuildPhotoAsync(Guid buildId, CancellationToken cancellationToken = default);
    }
}
