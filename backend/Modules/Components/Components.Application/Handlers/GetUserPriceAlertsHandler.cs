using Components.Application.Dtos;
using Components.Application.Queries;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Persistence;

namespace Components.Application.Handlers
{
    public class GetUserPriceAlertsHandler : IRequestHandler<GetUserPriceAlertsQuery, Result<List<UserPriceAlertDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserPriceAlertsHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<UserPriceAlertDto>>> Handle(GetUserPriceAlertsQuery request, CancellationToken cancellationToken)
        {
            var subscriptions = await _context.Set<PriceAlertSubscription>()
                .Where(s => s.UserId == request.UserId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);

            if (subscriptions.Count == 0)
                return Result.Success(new List<UserPriceAlertDto>());

            var result = new List<UserPriceAlertDto>(subscriptions.Count);

            foreach (var grouping in subscriptions.GroupBy(s => s.ComponentType))
            {
                var ids = grouping.Select(s => s.ComponentId).ToList();
                var meta = await LoadComponentMetaAsync(grouping.Key, ids, cancellationToken);

                foreach (var sub in grouping)
                {
                    meta.TryGetValue(sub.ComponentId, out var m);
                    result.Add(new UserPriceAlertDto(
                        sub.Id,
                        sub.ComponentId,
                        sub.ComponentType,
                        m.Name,
                        m.PhotoUrl,
                        sub.ThresholdPercent,
                        sub.InitialPrice,
                        sub.LastNotifiedPrice,
                        m.AveragePrice,
                        sub.CreatedAt));
                }
            }

            return Result.Success(result.OrderByDescending(r => r.CreatedAt).ToList());
        }

        private async Task<Dictionary<Guid, ComponentMeta>> LoadComponentMetaAsync(
            ComponentType type,
            List<Guid> ids,
            CancellationToken ct)
        {
            return type switch
            {
                ComponentType.Cpu => await _context.Set<Cpu>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.Gpu => await _context.Set<Gpu>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.Ram => await _context.Set<Ram>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.Motherboard => await _context.Set<Motherboard>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.CpuCooler => await _context.Set<CpuCooler>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.PcCase => await _context.Set<PcCase>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.PowerSupply => await _context.Set<PowerSupply>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.Ssd => await _context.Set<Ssd>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.Hdd => await _context.Set<Hdd>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                ComponentType.Fan => await _context.Set<Fan>()
                    .Where(c => ids.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name, c.PhotoUrl, c.AveragePrice })
                    .ToDictionaryAsync(c => c.Id, c => new ComponentMeta(c.Name, c.PhotoUrl, c.AveragePrice), ct),

                _ => new Dictionary<Guid, ComponentMeta>()
            };
        }

        private readonly record struct ComponentMeta(string? Name, string? PhotoUrl, decimal? AveragePrice);
    }
}
