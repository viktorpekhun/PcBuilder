using Auth.Domain.Entities;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Dtos;
using Moderation.Application.Queries;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace Moderation.Application.Handlers
{
    public class GetAdminStatsQueryHandler : IRequestHandler<GetAdminStatsQuery, Result<AdminStatsDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminStatsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<AdminStatsDto>> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);

            var totalUsers = await _context.Set<User>().CountAsync(cancellationToken);
            var newUsersLast7Days = await _context.Set<User>()
                .CountAsync(u => u.CreatedAt >= sevenDaysAgo, cancellationToken);

            var totalBuilds = await _context.Set<PcBuild>().CountAsync(cancellationToken);
            var publishedBuilds = await _context.Set<PcBuild>()
                .CountAsync(b => b.IsPublished, cancellationToken);

            var totalReviews = await _context.Set<Review>().CountAsync(cancellationToken);

            var pendingReports = await _context.Set<Report>()
                .CountAsync(r => r.Status == ReportStatus.Pending, cancellationToken);

            var activeBannedUsers = await _context.Set<User>()
                .CountAsync(u => u.CommentBanUntil > now || u.PostBanUntil > now, cancellationToken);

            var componentCounts = new Dictionary<string, int>
            {
                ["cpu"] = await _context.Set<Cpu>().CountAsync(cancellationToken),
                ["gpu"] = await _context.Set<Gpu>().CountAsync(cancellationToken),
                ["motherboard"] = await _context.Set<Motherboard>().CountAsync(cancellationToken),
                ["ram"] = await _context.Set<Ram>().CountAsync(cancellationToken),
                ["ssd"] = await _context.Set<Ssd>().CountAsync(cancellationToken),
                ["hdd"] = await _context.Set<Hdd>().CountAsync(cancellationToken),
                ["cpuCooler"] = await _context.Set<CpuCooler>().CountAsync(cancellationToken),
                ["fan"] = await _context.Set<Fan>().CountAsync(cancellationToken),
                ["pcCase"] = await _context.Set<PcCase>().CountAsync(cancellationToken),
                ["powerSupply"] = await _context.Set<PowerSupply>().CountAsync(cancellationToken),
            };

            return Result.Success(new AdminStatsDto
            {
                TotalUsers = totalUsers,
                NewUsersLast7Days = newUsersLast7Days,
                TotalBuilds = totalBuilds,
                PublishedBuilds = publishedBuilds,
                TotalReviews = totalReviews,
                PendingReports = pendingReports,
                ComponentCounts = componentCounts,
                ActiveBannedUsers = activeBannedUsers
            });
        }
    }
}