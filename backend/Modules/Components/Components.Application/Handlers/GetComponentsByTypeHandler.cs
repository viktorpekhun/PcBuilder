using AutoMapper;
using Components.Application.CollectionFilters;
using Components.Application.Dtos;
using Components.Application.Dtos.CpuDtos;
using Components.Application.Dtos.CpuCoolerDtos;
using Components.Application.Dtos.FanDtos;
using Components.Application.Dtos.GpuDtos;
using Components.Application.Dtos.HddDtos;
using Components.Application.Dtos.MotherboardDtos;
using Components.Application.Dtos.PcCaseDtos;
using Components.Application.Dtos.PowerSupplyDtos;
using Components.Application.Dtos.RamDtos;
using Components.Application.Dtos.SsdDtos;
using Components.Application.PreFilter;
using Components.Application.Queries;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Filtering;
using PcBuilder.SharedKernel.Persistence;

namespace Components.Application.Handlers
{
    public class GetComponentsByTypeHandler : IRequestHandler<GetComponentsByTypeQuery, Result<PagedResponse<object>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IComponentQueryPreFilter? _preFilter;
        private readonly IPowerSupplyCollectionFilterService _psuCollectionFilter;

        public GetComponentsByTypeHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IPowerSupplyCollectionFilterService psuCollectionFilter,
            IComponentQueryPreFilter? preFilter = null)
        {
            _context = context;
            _mapper = mapper;
            _preFilter = preFilter;
            _psuCollectionFilter = psuCollectionFilter;
        }

        public async Task<Result<PagedResponse<object>>> Handle(GetComponentsByTypeQuery request, CancellationToken cancellationToken)
        {
            PartialBuildContext? partialCtx = null;
            if (request.ApplyCompatibilityPrefilter && _preFilter != null && request.PartialBuildIds != null)
                partialCtx = await BuildPartialContext(request.PartialBuildIds, cancellationToken);

            (IEnumerable<object> componentEntities, int totalCount) = request.ComponentType switch
            {
                ComponentType.Cpu => await GetFilteredComponentsAndCount<Cpu>(
                    request.Parameters,
                    preFilter: partialCtx != null ? q => _preFilter!.FilterCpus(q, partialCtx) : null),

                ComponentType.Gpu => await GetFilteredComponentsAndCount<Gpu>(
                    request.Parameters,
                    include: q => q.Include(g => g.GpuPowerConnectors),
                    preFilter: partialCtx != null ? q => _preFilter!.FilterGpus(q, partialCtx) : null),

                ComponentType.Motherboard => await GetFilteredComponentsAndCount<Motherboard>(
                    request.Parameters,
                    include: q => q.Include(m => m.CpuPowerConnectors).Include(m => m.PcleSlots)
                                   .Include(m => m.M2Slots).Include(m => m.RearPorts).Include(m => m.InnerPorts),
                    preFilter: partialCtx != null ? q => _preFilter!.FilterMotherboards(q, partialCtx) : null),

                ComponentType.Ram => await GetFilteredComponentsAndCount<Ram>(
                    request.Parameters,
                    preFilter: partialCtx != null ? q => _preFilter!.FilterRams(q, partialCtx) : null),

                ComponentType.CpuCooler => await GetFilteredComponentsAndCount<CpuCooler>(
                    request.Parameters,
                    include: q => q.Include(c => c.CpuCoolerSockets),
                    preFilter: partialCtx != null ? q => _preFilter!.FilterCoolers(q, partialCtx) : null),

                ComponentType.PcCase => await GetFilteredComponentsAndCount<PcCase>(
                    request.Parameters,
                    include: q => q.Include(c => c.PcCaseFormFactors).Include(c => c.PcCaseFanLocations),
                    preFilter: partialCtx != null ? q => _preFilter!.FilterCases(q, partialCtx) : null),

                ComponentType.PowerSupply => await GetFilteredComponentsAndCount<PowerSupply>(
                    request.Parameters,
                    include: q => q.Include(p => p.PowerSupplyPowerConnectors),
                    collectionFilter: q => _psuCollectionFilter.Apply(q, request.Parameters),
                    preFilter: partialCtx != null ? q => _preFilter!.FilterPowerSupplies(q, partialCtx) : null),

                ComponentType.Ssd => await GetFilteredComponentsAndCount<Ssd>(request.Parameters),
                ComponentType.Hdd => await GetFilteredComponentsAndCount<Hdd>(request.Parameters),

                ComponentType.Fan => await GetFilteredComponentsAndCount<Fan>(
                    request.Parameters,
                    preFilter: partialCtx != null ? q => _preFilter!.FilterFans(q, partialCtx) : null),

                _ => (Enumerable.Empty<object>(), 0)
            };

            List<object> mappedItems = request.ComponentType switch
            {
                ComponentType.Cpu => MapList<Cpu, CpuListDto>(componentEntities),
                ComponentType.Gpu => MapList<Gpu, GpuListDto>(componentEntities),
                ComponentType.Motherboard => MapList<Motherboard, MotherboardListDto>(componentEntities),
                ComponentType.Ram => MapList<Ram, RamListDto>(componentEntities),
                ComponentType.CpuCooler => MapList<CpuCooler, CpuCoolerListDto>(componentEntities),
                ComponentType.PcCase => MapList<PcCase, PcCaseListDto>(componentEntities),
                ComponentType.PowerSupply => MapList<PowerSupply, PowerSupplyListDto>(componentEntities),
                ComponentType.Ssd => MapList<Ssd, SsdListDto>(componentEntities),
                ComponentType.Hdd => MapList<Hdd, HddListDto>(componentEntities),
                ComponentType.Fan => MapList<Fan, FanListDto>(componentEntities),
                _ => throw new InvalidOperationException("Unreachable")
            };

            return Result<PagedResponse<object>>.Success(
                new PagedResponse<object>(mappedItems, totalCount, request.Parameters));
        }

        private List<object> MapList<TEntity, TDto>(IEnumerable<object> entities)
            where TEntity : class
            where TDto : IComponentListDto
        {
            return _mapper.Map<List<TDto>>(entities.Cast<TEntity>()).Cast<object>().ToList();
        }

        private async Task<(IEnumerable<object>, int)> GetFilteredComponentsAndCount<T>(
            ResourceParameters parameters,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IQueryable<T>>? collectionFilter = null,
            Func<IQueryable<T>, IQueryable<T>>? preFilter = null) where T : class
        {
            var query = _context.Set<T>().AsQueryable();

            if (preFilter != null)
                query = preFilter(query);

            if (collectionFilter != null)
                query = collectionFilter(query);

            var result = await query.FilterAndPageAsync(parameters, include: include);
            return (result.items.Cast<object>(), result.totalCount);
        }

        private async Task<PartialBuildContext> BuildPartialContext(
            PartialBuildIds ids,
            CancellationToken ct)
        {
            Cpu? cpu = ids.CpuId.HasValue
                ? await _context.Set<Cpu>().FirstOrDefaultAsync(c => c.Id == ids.CpuId.Value, ct)
                : null;

            Gpu? gpu = ids.GpuId.HasValue
                ? await _context.Set<Gpu>()
                    .Include(g => g.GpuPowerConnectors)
                    .FirstOrDefaultAsync(g => g.Id == ids.GpuId.Value, ct)
                : null;

            Motherboard? mb = ids.MotherboardId.HasValue
                ? await _context.Set<Motherboard>()
                    .Include(m => m.CpuPowerConnectors)
                    .Include(m => m.PcleSlots)
                    .Include(m => m.M2Slots)
                    .FirstOrDefaultAsync(m => m.Id == ids.MotherboardId.Value, ct)
                : null;

            Ram? ram = ids.RamId.HasValue
                ? await _context.Set<Ram>().FirstOrDefaultAsync(r => r.Id == ids.RamId.Value, ct)
                : null;

            CpuCooler? cooler = ids.CpuCoolerId.HasValue
                ? await _context.Set<CpuCooler>()
                    .Include(c => c.CpuCoolerSockets)
                    .FirstOrDefaultAsync(c => c.Id == ids.CpuCoolerId.Value, ct)
                : null;

            PcCase? pcCase = ids.PcCaseId.HasValue
                ? await _context.Set<PcCase>()
                    .Include(c => c.PcCaseFormFactors)
                    .Include(c => c.PcCaseFanLocations)
                    .FirstOrDefaultAsync(c => c.Id == ids.PcCaseId.Value, ct)
                : null;

            PowerSupply? psu = ids.PowerSupplyId.HasValue
                ? await _context.Set<PowerSupply>()
                    .Include(p => p.PowerSupplyPowerConnectors)
                    .FirstOrDefaultAsync(p => p.Id == ids.PowerSupplyId.Value, ct)
                : null;

            return new PartialBuildContext(cpu, gpu, mb, ram, cooler, pcCase, psu);
        }
    }
}
