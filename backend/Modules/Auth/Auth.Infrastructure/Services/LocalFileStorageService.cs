using Auth.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _avatarDirectory;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _avatarDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                configuration["Files:AvatarDirectory"] ?? Path.Combine("wwwroot", "avatars"));

            Directory.CreateDirectory(_avatarDirectory);
        }

        public async Task<string> SaveAvatarAsync(Guid userId, byte[] data, string extension, CancellationToken cancellationToken = default)
        {
            await DeleteAvatarAsync(userId, cancellationToken);

            var fileName = $"{userId}.{extension}";
            var filePath = Path.Combine(_avatarDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, data, cancellationToken);
            return $"/avatars/{fileName}";
        }

        public Task DeleteAvatarAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            foreach (var file in Directory.GetFiles(_avatarDirectory, $"{userId}.*"))
                File.Delete(file);
            return Task.CompletedTask;
        }
    }
}
