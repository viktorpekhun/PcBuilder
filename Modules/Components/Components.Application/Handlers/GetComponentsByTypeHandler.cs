using AutoMapper;
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
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Filtering;
using PcBuilder.SharedKernel.Persistence;
using Components.Application.Queries;

namespace Components.Application.Handlers
{
    public class GetComponentsByTypeHandler : IRequestHandler<GetComponentsByTypeQuery, Result<PagedResponse<IComponentListDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetComponentsByTypeHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedResponse<IComponentListDto>>> Handle(GetComponentsByTypeQuery request, CancellationToken cancellationToken)
        {
            (IEnumerable<object> componentEntities, int totalCount) = request.ComponentType switch
            {
                ComponentType.Cpu => await GetFilteredComponentsAndCount<Cpu>(request.Parameters),
                ComponentType.Gpu => await GetFilteredComponentsAndCount<Gpu>(request.Parameters, q => q.Include(g => g.GpuPowerConnectors)),
                ComponentType.Motherboard => await GetFilteredComponentsAndCount<Motherboard>(request.Parameters, q => q
                    .Include(m => m.CpuPowerConnectors).Include(m => m.PcleSlots).Include(m => m.M2Slots)
                    .Include(m => m.RearPorts).Include(m => m.InnerPorts)),
                ComponentType.Ram => await GetFilteredComponentsAndCount<Ram>(request.Parameters),
                ComponentType.CpuCooler => await GetFilteredComponentsAndCount<CpuCooler>(request.Parameters, q => q.Include(c => c.CpuCoolerSockets)),
                ComponentType.PcCase => await GetFilteredComponentsAndCount<PcCase>(request.Parameters, q => q
                    .Include(c => c.PcCaseFormFactors).Include(c => c.PcCaseFanLocations)),
                ComponentType.PowerSupply => await GetFilteredComponentsAndCount<PowerSupply>(request.Parameters, q => q.Include(p => p.PowerSupplyPowerConnectors)),
                ComponentType.Ssd => await GetFilteredComponentsAndCount<Ssd>(request.Parameters),
                ComponentType.Hdd => await GetFilteredComponentsAndCount<Hdd>(request.Parameters),
                ComponentType.Fan => await GetFilteredComponentsAndCount<Fan>(request.Parameters),
                _ => (Enumerable.Empty<object>(), 0)
            };

            List<IComponentListDto> mappedItems = request.ComponentType switch
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

            return Result<PagedResponse<IComponentListDto>>.Success(
                new PagedResponse<IComponentListDto>(mappedItems, totalCount, request.Parameters));
        }

        private List<IComponentListDto> MapList<TEntity, TDto>(IEnumerable<object> entities)
            where TEntity : class
            where TDto : IComponentListDto
        {
            return _mapper.Map<List<TDto>>(entities.Cast<TEntity>()).Cast<IComponentListDto>().ToList();
        }

        private async Task<(IEnumerable<object>, int)> GetFilteredComponentsAndCount<T>(
            ResourceParameters parameters,
            Func<IQueryable<T>, IQueryable<T>>? include = null) where T : class
        {
            var result = await _context.Set<T>()
                .AsQueryable()
                .FilterAndPageAsync(parameters, include: include);

            return (result.items.Cast<object>(), result.totalCount);
        }
    }
}
