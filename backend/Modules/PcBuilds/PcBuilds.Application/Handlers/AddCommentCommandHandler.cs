using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Commands;
using Notifications.Domain;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _sender;

        public AddCommentCommandHandler(IApplicationDbContext context, ISender sender)
        {
            _context = context;
            _sender = sender;
        }

        public async Task<Result<Guid>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

            if (build == null)
                return Result.Failure<Guid>(new Error("NotFound", "Build not found.", 404));

            if (!build.IsPublished && build.UserId != request.UserId)
                return Result.Failure<Guid>(new Error("Forbidden", "You cannot comment on this build.", 403));

            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result.Failure<Guid>(new Error("NotFound", "User not found.", 404));

            if (user.CommentBanUntil > DateTime.UtcNow)
                return Result.Failure<Guid>(new Error("Forbidden", "You are temporarily banned from commenting.", 403));

            var review = new Review
            {
                Text = request.Text,
                Rating = request.Rating,
                UserId = request.UserId,
                PcBuildId = request.PcBuildId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Set<Review>().AddAsync(review, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var avgRating = await _context.Set<Review>()
                .Where(r => r.PcBuildId == request.PcBuildId)
                .AverageAsync(r => r.Rating, cancellationToken);

            build.AverageRating = Math.Round(avgRating, 2);
            await _context.SaveChangesAsync(cancellationToken);

            if (build.UserId != request.UserId)
            {
                await _sender.Send(new CreateNotificationCommand(
                    build.UserId,
                    NotificationTypes.NewReview,
                    new Dictionary<string, string>
                    {
                        ["buildId"] = build.Id.ToString(),
                        ["buildName"] = build.Name,
                        ["reviewerUsername"] = user.Username
                    }), cancellationToken);
            }

            return Result.Success(review.Id);
        }
    }
}