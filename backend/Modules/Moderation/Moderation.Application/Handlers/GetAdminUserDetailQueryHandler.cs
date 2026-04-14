using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Dtos;
using Moderation.Application.Queries;
using Moderation.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace Moderation.Application.Handlers
{
    public class GetAdminUserDetailQueryHandler : IRequestHandler<GetAdminUserDetailQuery, Result<AdminUserDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminUserDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<AdminUserDetailDto>> Handle(GetAdminUserDetailQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result.Failure<AdminUserDetailDto>(new Error("NotFound", "User not found.", 404));

            var now = DateTime.UtcNow;

            var warnings = await _context.Set<Warning>()
                .Where(w => w.UserId == request.UserId)
                .OrderByDescending(w => w.IssuedAt)
                .Select(w => new WarningDto
                {
                    Id = w.Id,
                    BanType = w.BanType,
                    Reason = w.Reason,
                    IssuedByAdminUsername = w.IssuedByAdmin != null ? w.IssuedByAdmin.Username : null,
                    IssuedAt = w.IssuedAt
                })
                .ToListAsync(cancellationToken);

            var buildCount = await _context.Set<PcBuild>()
                .CountAsync(b => b.UserId == request.UserId, cancellationToken);

            var reviewCount = await _context.Set<Review>()
                .CountAsync(r => r.UserId == request.UserId, cancellationToken);

            return Result.Success(new AdminUserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                Roles = user.Roles.Select(r => r.Name).ToList(),
                CreatedAt = user.CreatedAt,
                IsEmailVerified = user.IsEmailVerified,
                CommentBanUntil = user.CommentBanUntil > now ? user.CommentBanUntil : null,
                PostBanUntil = user.PostBanUntil > now ? user.PostBanUntil : null,
                IsCommentBanned = user.CommentBanUntil > now,
                IsPostBanned = user.PostBanUntil > now,
                Warnings = warnings,
                BuildCount = buildCount,
                ReviewCount = reviewCount
            });
        }
    }
}