using AutoMapper;
using Components.Application.Dtos;
using Components.Application.Queries;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Components.Application.Handlers
{
    public class GetPriceHistoryHandler : IRequestHandler<GetPriceHistoryQuery, Result<List<PriceHistoryPointDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetPriceHistoryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<PriceHistoryPointDto>>> Handle(GetPriceHistoryQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Set<PriceHistoryEntry>()
                .Where(p => p.ComponentId == request.Id && p.ComponentType == request.ComponentType);

            if (request.Days > 0)
            {
                var cutoff = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-request.Days);
                query = query.Where(p => p.SnapshotDate >= cutoff);
            }

            var entries = await query
                .OrderBy(p => p.SnapshotDate)
                .ToListAsync(cancellationToken);

            var points = _mapper.Map<List<PriceHistoryPointDto>>(entries);
            return Result<List<PriceHistoryPointDto>>.Success(points);
        }
    }
}
