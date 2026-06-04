using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Moderation.Application.Services;
using Notifications.Application.Commands;
using Notifications.Domain;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace Moderation.Application.Handlers
{
    public class AdminDeleteReviewCommandHandler : IRequestHandler<AdminDeleteReviewCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _sender;
        private readonly IAdminActivityLogger _activity;

        public AdminDeleteReviewCommandHandler(IApplicationDbContext context, ISender sender, IAdminActivityLogger activity)
        {
            _context = context;
            _sender = sender;
            _activity = activity;
        }

        public async Task<Result<bool>> Handle(AdminDeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _context.Set<Review>()
                .Include(r => r.PcBuild)
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            if (review == null)
                return Result.Failure<bool>(new Error("NotFound", "Review not found.", 404));

            var buildId = review.PcBuildId;
            var userId = review.UserId;
            var buildName = review.PcBuild?.Name ?? "Unknown";

            _context.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);

            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == buildId, cancellationToken);

            if (build != null)
            {
                var reviews = await _context.Set<Review>()
                    .Where(r => r.PcBuildId == buildId)
                    .ToListAsync(cancellationToken);

                build.AverageRating = reviews.Count > 0
                    ? Math.Round(reviews.Average(r => r.Rating), 2)
                    : 0;

                await _context.SaveChangesAsync(cancellationToken);
            }

            if (request.NotifyUser)
            {
                await _sender.Send(new CreateNotificationCommand(
                    userId,
                    NotificationTypes.ReviewDeleted,
                    new Dictionary<string, string>
                    {
                        ["buildId"] = buildId.ToString(),
                        ["buildName"] = buildName
                    }), cancellationToken);
            }

            await _activity.LogAsync(
                request.AdminId,
                "DeleteReview",
                targetType: "Review",
                targetId: request.ReviewId,
                targetName: $"review on {buildName}",
                cancellationToken: cancellationToken);

            return Result.Success(true);
        }
    }
}