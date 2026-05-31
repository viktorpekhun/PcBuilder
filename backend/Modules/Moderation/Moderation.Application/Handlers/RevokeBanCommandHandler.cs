using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Moderation.Application.Services;
using Moderation.Domain.Enums;
using Notifications.Application.Commands;
using Notifications.Domain;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Moderation.Application.Handlers
{
    public class RevokeBanCommandHandler : IRequestHandler<RevokeBanCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _sender;
        private readonly IAdminActivityLogger _activity;

        public RevokeBanCommandHandler(IApplicationDbContext context, ISender sender, IAdminActivityLogger activity)
        {
            _context = context;
            _sender = sender;
            _activity = activity;
        }

        public async Task<Result<bool>> Handle(RevokeBanCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result.Failure<bool>(new Error("NotFound", "User not found.", 404));

            var now = DateTime.UtcNow;

            if (request.BanType == BanType.Comment)
            {
                if (user.CommentBanUntil <= now)
                    return Result.Failure<bool>(new Error("NotBanned", "User is not currently comment-banned.", 400));

                user.CommentBanUntil = now;
            }
            else
            {
                if (user.PostBanUntil <= now)
                    return Result.Failure<bool>(new Error("NotBanned", "User is not currently post-banned.", 400));

                user.PostBanUntil = now;
            }

            await _context.SaveChangesAsync(cancellationToken);

            await _sender.Send(new CreateNotificationCommand(
                request.UserId,
                request.BanType == BanType.Comment ? NotificationTypes.CommentUnbanned : NotificationTypes.PostUnbanned,
                new Dictionary<string, string>()), cancellationToken);

            await _activity.LogAsync(
                request.AdminId,
                "UnbanUser",
                targetType: "User",
                targetId: user.Id,
                targetName: $"@{user.Username}",
                detail: $"{request.BanType} ban revoked",
                cancellationToken: cancellationToken);

            return Result.Success(true);
        }
    }
}
