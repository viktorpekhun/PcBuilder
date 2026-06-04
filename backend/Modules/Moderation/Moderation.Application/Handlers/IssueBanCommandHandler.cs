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
    public class IssueBanCommandHandler : IRequestHandler<IssueBanCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _sender;
        private readonly IAdminActivityLogger _activity;

        public IssueBanCommandHandler(IApplicationDbContext context, ISender sender, IAdminActivityLogger activity)
        {
            _context = context;
            _sender = sender;
            _activity = activity;
        }

        public async Task<Result<bool>> Handle(IssueBanCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result.Failure<bool>(new Error("NotFound", "User not found.", 404));

            var banUntil = DateTime.UtcNow.AddDays(request.DurationDays);

            if (request.BanType == BanType.Comment)
                user.CommentBanUntil = banUntil;
            else
                user.PostBanUntil = banUntil;

            await _context.SaveChangesAsync(cancellationToken);

            await _sender.Send(new CreateNotificationCommand(
                request.UserId,
                request.BanType == BanType.Comment ? NotificationTypes.CommentBanned : NotificationTypes.PostBanned,
                new Dictionary<string, string>
                {
                    ["banUntil"] = banUntil.ToString("O"),
                    ["reason"] = request.Reason
                }), cancellationToken);

            await _activity.LogAsync(
                request.AdminId,
                "BanUser",
                targetType: "User",
                targetId: user.Id,
                targetName: $"@{user.Username}",
                detail: $"{request.BanType} ban · {request.DurationDays}d · {request.Reason}",
                cancellationToken: cancellationToken);

            return Result.Success(true);
        }
    }
}