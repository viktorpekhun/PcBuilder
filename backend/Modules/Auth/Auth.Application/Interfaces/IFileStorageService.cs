namespace Auth.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAvatarAsync(Guid userId, byte[] data, CancellationToken cancellationToken = default);
        Task DeleteAvatarAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
