using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Dtos;
using Moderation.Application.Queries;
using Moderation.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;
using PcBuilder.SharedKernel.Persistence;

namespace Moderation.Application.Handlers
{
    public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, Result<PagedResponse<AdminUserDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminUsersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResponse<AdminUserDto>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Set<User>()
                .Include(u => u.Roles)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                var search = request.SearchQuery.ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userIds = users.Select(u => u.Id).ToList();
            var warningCounts = await _context.Set<Warning>()
                .Where(w => userIds.Contains(w.UserId))
                .GroupBy(w => w.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken);

            var items = users.Select(u => new AdminUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Roles = u.Roles.Select(r => r.Name).ToList(),
                CreatedAt = u.CreatedAt,
                CommentBanUntil = u.CommentBanUntil > now ? u.CommentBanUntil : null,
                PostBanUntil = u.PostBanUntil > now ? u.PostBanUntil : null,
                IsCommentBanned = u.CommentBanUntil > now,
                IsPostBanned = u.PostBanUntil > now,
                WarningsCount = warningCounts.GetValueOrDefault(u.Id, 0)
            }).ToList();

            var parameters = new ResourceParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(new PagedResponse<AdminUserDto>(items, totalCount, parameters));
        }
    }
}