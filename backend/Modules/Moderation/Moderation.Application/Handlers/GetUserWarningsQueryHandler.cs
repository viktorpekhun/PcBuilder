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
    public class GetUserWarningsQueryHandler : IRequestHandler<GetUserWarningsQuery, Result<List<WarningDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserWarningsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<WarningDto>>> Handle(GetUserWarningsQuery request, CancellationToken cancellationToken)
        {
            var warnings = await _context.Set<Warning>()
                .Where(w => w.UserId == request.UserId)
                .OrderByDescending(w => w.IssuedAt)
                .ToListAsync(cancellationToken);

            var adminIds = warnings
                .Where(w => w.IssuedByAdminId.HasValue)
                .Select(w => w.IssuedByAdminId!.Value)
                .Distinct()
                .ToList();

            var admins = await _context.Set<User>()
                .Where(u => adminIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username, cancellationToken);

            return Result.Success(warnings.Select(w => new WarningDto
            {
                Id = w.Id,
                BanType = w.BanType,
                Reason = w.Reason,
                IssuedByAdminUsername = w.IssuedByAdminId.HasValue
                    ? admins.GetValueOrDefault(w.IssuedByAdminId.Value)
                    : null,
                IssuedAt = w.IssuedAt
            }).ToList());
        }
    }
}