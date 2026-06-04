using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Dtos;
using Moderation.Application.Queries;
using Moderation.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Moderation.Application.Handlers
{
    public class GetUserBansQueryHandler : IRequestHandler<GetUserBansQuery, Result<UserBanStatusDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserBansQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<UserBanStatusDto>> Handle(GetUserBansQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result.Failure<UserBanStatusDto>(new Error("NotFound", "User not found.", 404));

            var cutoff = DateTime.UtcNow.AddDays(-180);
            var recentWarnings = await _context.Set<Warning>()
                .Where(w => w.UserId == request.UserId && w.IssuedAt >= cutoff)
                .OrderByDescending(w => w.IssuedAt)
                .Select(w => new WarningDto
                {
                    Id = w.Id,
                    BanType = w.BanType,
                    Reason = w.Reason,
                    ReasonCode = w.ReasonCode,
                    IssuedAt = w.IssuedAt
                })
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            return Result.Success(new UserBanStatusDto
            {
                CommentBanUntil = user.CommentBanUntil > now ? user.CommentBanUntil : null,
                PostBanUntil = user.PostBanUntil > now ? user.PostBanUntil : null,
                IsCommentBanned = user.CommentBanUntil > now,
                IsPostBanned = user.PostBanUntil > now,
                RecentWarnings = recentWarnings
            });
        }
    }
}