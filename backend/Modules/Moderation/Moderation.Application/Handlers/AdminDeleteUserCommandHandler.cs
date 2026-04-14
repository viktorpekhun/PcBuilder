using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Moderation.Domain.Entities;
using Notifications.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace Moderation.Application.Handlers
{
    public class AdminDeleteUserCommandHandler : IRequestHandler<AdminDeleteUserCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminDeleteUserCommandHandler(IApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Result<bool>> Handle(AdminDeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken);

            if (user == null)
                return Result.Failure<bool>(new Error("NotFound", "User not found.", 404));

            if (user.Id == request.AdminId)
                return Result.Failure<bool>(new Error("Forbidden", "You cannot delete your own account through admin actions.", 403));

            // 1. Send account deletion email before any DB changes
            await _emailService.SendAccountDeletionEmailAsync(user.Email, cancellationToken);

            // 2. Delete reviews authored by this user on OTHER users' builds
            //    and recalculate average ratings for affected builds
            var userReviews = await _context.Set<Review>()
                .Where(r => r.UserId == request.TargetUserId)
                .ToListAsync(cancellationToken);

            var affectedBuildIds = userReviews
                .Select(r => r.PcBuildId)
                .Distinct()
                .ToList();

            foreach (var review in userReviews)
                _context.Remove(review);

            await _context.SaveChangesAsync(cancellationToken);

            // Recalculate average ratings for affected builds
            foreach (var buildId in affectedBuildIds)
            {
                var build = await _context.Set<PcBuild>()
                    .FirstOrDefaultAsync(b => b.Id == buildId, cancellationToken);

                if (build != null)
                {
                    var remaining = await _context.Set<Review>()
                        .Where(r => r.PcBuildId == buildId)
                        .ToListAsync(cancellationToken);

                    build.AverageRating = remaining.Count > 0
                        ? Math.Round(remaining.Average(r => r.Rating), 2)
                        : 0;
                }
            }

            // 3. Delete all notifications for this user
            var notifications = await _context.Set<Notification>()
                .Where(n => n.UserId == request.TargetUserId)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
                _context.Remove(notification);

            // 4. Delete all warnings where this user is the target OR the issuing admin.
            //    FKs are Restrict, so every Warning row referencing the user must go first.
            var warnings = await _context.Set<Warning>()
                .Where(w => w.UserId == request.TargetUserId
                         || w.IssuedByAdminId == request.TargetUserId)
                .ToListAsync(cancellationToken);

            foreach (var warning in warnings)
                _context.Remove(warning);

            // 5. Delete all reports where this user is the reporter, the reported user,
            //    or the resolving admin. FKs are Restrict — must delete, not dismiss.
            var relatedReports = await _context.Set<Report>()
                .Where(r => r.ReporterId == request.TargetUserId
                         || r.ReportedUserId == request.TargetUserId
                         || r.ResolvedByAdminId == request.TargetUserId)
                .ToListAsync(cancellationToken);

            foreach (var report in relatedReports)
                _context.Remove(report);

            // 7. Delete the user — cascades: PcBuilds → Reviews on those builds
            _context.Set<User>().Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(true);
        }
    }
}