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
    public class GetAdminActivityQueryHandler : IRequestHandler<GetAdminActivityQuery, Result<PagedResponse<AdminActivityLogDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminActivityQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResponse<AdminActivityLogDto>>> Handle(GetAdminActivityQuery request, CancellationToken cancellationToken)
        {
            var since = DateTime.UtcNow.AddDays(-request.DaysBack);

            var query = _context.Set<AdminActivityLog>()
                .Where(a => a.OccurredAt >= since)
                .OrderByDescending(a => a.OccurredAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var logs = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var adminIds = logs.Select(l => l.AdminId).Distinct().ToList();
            var admins = await _context.Set<User>()
                .Where(u => adminIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username, cancellationToken);

            var items = logs.Select(l => new AdminActivityLogDto
            {
                Id             = l.Id,
                AdminUsername  = admins.GetValueOrDefault(l.AdminId, "deleted-admin"),
                Action         = l.Action,
                TargetType     = l.TargetType,
                TargetId       = l.TargetId,
                TargetName     = l.TargetName,
                Detail         = l.Detail,
                OccurredAt     = l.OccurredAt,
            }).ToList();

            var parameters = new ResourceParameters
            {
                PageNumber = request.PageNumber,
                PageSize   = request.PageSize,
            };

            return Result.Success(new PagedResponse<AdminActivityLogDto>(items, totalCount, parameters));
        }
    }
}
