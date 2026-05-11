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
    public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, Result<PagedResponse<ReportDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetReportsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResponse<ReportDto>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Set<Report>().AsQueryable();

            if (request.Status.HasValue)
                query = query.Where(r => r.Status == request.Status.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var reports = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userIds = reports
                .SelectMany(r => new[] { r.ReporterId, r.ReportedUserId })
                .Distinct()
                .ToList();

            var users = await _context.Set<User>()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username, cancellationToken);

            var items = reports.Select(r => new ReportDto
            {
                Id = r.Id,
                ReporterId = r.ReporterId,
                ReporterUsername = users.GetValueOrDefault(r.ReporterId, "Deleted User"),
                ReportType = r.ReportType,
                ReportedEntityId = r.ReportedEntityId,
                ReportedUserId = r.ReportedUserId,
                ReportedUsername = users.GetValueOrDefault(r.ReportedUserId, "Deleted User"),
                Reason = r.Reason,
                Status = r.Status,
                AdminResolutionNote = r.AdminResolutionNote,
                ResolvedAt = r.ResolvedAt,
                CreatedAt = r.CreatedAt
            }).ToList();

            var parameters = new ResourceParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(new PagedResponse<ReportDto>(items, totalCount, parameters));
        }
    }
}