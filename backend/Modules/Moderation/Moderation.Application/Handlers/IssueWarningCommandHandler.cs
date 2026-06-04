using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using Notifications.Application.Commands;
using Notifications.Domain;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Moderation.Application.Handlers
{
    public class IssueWarningCommandHandler : IRequestHandler<IssueWarningCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISender _sender;

        private static readonly int[] BanDurationDays = [0, 0, 1, 3, 7, 30];

        public IssueWarningCommandHandler(IApplicationDbContext context, ISender sender)
        {
            _context = context;
            _sender = sender;
        }

        public async Task<Result<Guid>> Handle(IssueWarningCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result.Failure<Guid>(new Error("NotFound", "User not found.", 404));

            var warning = new Warning
            {
                UserId = request.UserId,
                BanType = request.BanType,
                ReasonCode = request.ReasonCode,
                IssuedByAdminId = request.AdminId,
                IssuedAt = DateTime.UtcNow
            };

            await _context.Set<Warning>().AddAsync(warning, cancellationToken);

            // Count warnings in the last 180 days (including the new one)
            var cutoff = DateTime.UtcNow.AddDays(-180);
            var recentWarnings = await _context.Set<Warning>()
                .CountAsync(w => w.UserId == request.UserId
                    && w.BanType == request.BanType
                    && w.IssuedAt >= cutoff, cancellationToken) + 1; // +1 for the one just added but not yet saved

            await _context.SaveChangesAsync(cancellationToken);

            await _sender.Send(new CreateNotificationCommand(
                request.UserId,
                request.BanType == BanType.Comment ? NotificationTypes.CommentWarning : NotificationTypes.PostWarning,
                new Dictionary<string, string>
                {
                    ["reason"] = request.ReasonCode,
                    ["warningsCount"] = recentWarnings.ToString()
                }), cancellationToken);

            // Auto-ban escalation
            if (recentWarnings >= 3)
            {
                var durationIndex = Math.Min(recentWarnings, BanDurationDays.Length - 1);
                var banDays = BanDurationDays[durationIndex];
                var banUntil = DateTime.UtcNow.AddDays(banDays);

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
                        ["reason"] = "AUTO_BAN_WARNINGS",
                        ["warningsCount"] = recentWarnings.ToString()
                    }), cancellationToken);
            }

            return Result.Success(warning.Id);
        }
    }
}