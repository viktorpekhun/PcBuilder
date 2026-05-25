using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Filtering;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Api.Search
{
    public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, List<GlobalSearchItemDto>>
    {
        private readonly IApplicationDbContext _context;

        public GlobalSearchQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<GlobalSearchItemDto>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
        {
            var q = request.Query;
            var limit = request.Limit;

            var results = new List<GlobalSearchItemDto>();

            results.AddRange(RankAndMap(await FetchNames<Cpu>(cancellationToken), q, limit, "CPU", "cpu"));
            results.AddRange(RankAndMap(await FetchNames<Gpu>(cancellationToken), q, limit, "GPU", "gpu"));
            results.AddRange(RankAndMap(await FetchNames<Ram>(cancellationToken), q, limit, "RAM", "ram"));
            results.AddRange(RankAndMap(await FetchNames<Motherboard>(cancellationToken), q, limit, "Motherboard", "motherboard"));
            results.AddRange(RankAndMap(await FetchNames<CpuCooler>(cancellationToken), q, limit, "CPU Cooler", "cpucooler"));
            results.AddRange(RankAndMap(await FetchNames<PcCase>(cancellationToken), q, limit, "Case", "pccase"));
            results.AddRange(RankAndMap(await FetchNames<PowerSupply>(cancellationToken), q, limit, "Power Supply", "powersupply"));
            results.AddRange(RankAndMap(await FetchNames<Ssd>(cancellationToken), q, limit, "SSD", "ssd"));
            results.AddRange(RankAndMap(await FetchNames<Hdd>(cancellationToken), q, limit, "HDD", "hdd"));
            results.AddRange(RankAndMap(await FetchNames<Fan>(cancellationToken), q, limit, "Fan", "fan"));

            var builds = await _context.Set<PcBuild>()
                .Where(b => b.IsPublished)
                .Select(b => new NameEntry(b.Id, b.Name))
                .ToListAsync(cancellationToken);
            results.AddRange(RankAndMap(builds, q, limit, "Build", null));

            return results;
        }

        private async Task<List<NameEntry>> FetchNames<T>(CancellationToken ct) where T : class
        {
            // Use anonymous type so EF Core can reliably translate the projection to SQL.
            // Projecting directly to a private record type can result in null Name values.
            var raw = await _context.Set<T>()
                .Select(e => new { Id = EF.Property<Guid>(e, "Id"), Name = EF.Property<string>(e, "Name") })
                .ToListAsync(ct);

            return raw.ConvertAll(x => new NameEntry(x.Id, x.Name));
        }

        private static List<GlobalSearchItemDto> RankAndMap(
            List<NameEntry> entries, string query, int limit, string category, string? componentType)
        {
            return FuzzySearchHelper
                .RankAndFilter(entries, query, e => e.Name, minScore: 60)
                .Take(limit)
                .Select(e =>
                {
                    var navigateTo = componentType != null
                        ? $"/components/{componentType}/{e.Id}"
                        : $"/builds/{e.Id}";
                    return new GlobalSearchItemDto(e.Id, e.Name, category, navigateTo);
                })
                .ToList();
        }

        private record NameEntry(Guid Id, string Name);
    }
}
