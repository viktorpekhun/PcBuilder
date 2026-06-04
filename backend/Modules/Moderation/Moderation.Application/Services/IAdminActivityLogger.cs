namespace Moderation.Application.Services
{
    public interface IAdminActivityLogger
    {
        Task LogAsync(
            Guid adminId,
            string action,
            string? targetType = null,
            Guid? targetId = null,
            string? targetName = null,
            string? detail = null,
            CancellationToken cancellationToken = default);
    }
}
