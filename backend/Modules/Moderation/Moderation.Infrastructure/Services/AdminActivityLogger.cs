using Moderation.Application.Services;
using Moderation.Domain.Entities;
using PcBuilder.SharedKernel.Persistence;

namespace Moderation.Infrastructure.Services
{
    public class AdminActivityLogger : IAdminActivityLogger
    {
        private readonly IApplicationDbContext _context;

        public AdminActivityLogger(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(
            Guid adminId,
            string action,
            string? targetType = null,
            Guid? targetId = null,
            string? targetName = null,
            string? detail = null,
            CancellationToken cancellationToken = default)
        {
            _context.Set<AdminActivityLog>().Add(new AdminActivityLog
            {
                AdminId    = adminId,
                Action     = action,
                TargetType = targetType,
                TargetId   = targetId,
                TargetName = targetName,
                Detail     = detail,
                OccurredAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
