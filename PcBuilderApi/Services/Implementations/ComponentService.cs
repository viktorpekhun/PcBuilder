using PcBuilderApi.Mappers;
using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Services.Interfaces;
using PcBuilderApi.Utilities.Filtering;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Implementations
{
    public class ComponentService : IComponentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IComponentMapper> _detailsMappers;
        private readonly IEnumerable<IComponentListMapper> _listMappers;

        public ComponentService(IUnitOfWork unitOfWork, IEnumerable<IComponentMapper> detailsMappers, 
            IEnumerable<IComponentListMapper> listMappers)
        {
            _unitOfWork = unitOfWork;
            _detailsMappers = detailsMappers;
            _listMappers = listMappers;
        }

        public async Task<PagedResponse<object>> GetAllByTypeAsync(ComponentType componentType, ResourceParameters parameters)
        {
            var mapper = _listMappers.FirstOrDefault(m => m.ComponentType == componentType);
            if (mapper == null) throw new ArgumentException("Unsupported component type");

            // Get filtered, sorted and paged components
            (IEnumerable<object> componentEntities, int totalCount) = componentType switch
            {
                ComponentType.Cpu => await GetFilteredComponentsAndCount<Cpu>(parameters),
                ComponentType.Gpu => await GetFilteredComponentsAndCount<Gpu>(parameters, "GpuPowerConnectors"),
                ComponentType.Motherboard => await GetFilteredComponentsAndCount<Motherboard>(
                    parameters, "CpuPowerConnectors,PcleSlots,M2Slots,RearPorts,InnerPorts"),
                ComponentType.Ram => await GetFilteredComponentsAndCount<Ram>(parameters),
                ComponentType.CpuCooler => await GetFilteredComponentsAndCount<CpuCooler>(
                    parameters, "CpuCoolerSockets"),
                ComponentType.PcCase => await GetFilteredComponentsAndCount<PcCase>(
                    parameters, "PcCaseFormFactors,PcCaseFanLocations"),
                ComponentType.PowerSupply => await GetFilteredComponentsAndCount<PowerSupply>(
                    parameters, "PowerSupplyPowerConnectors"),
                ComponentType.Ssd => await GetFilteredComponentsAndCount<Ssd>(parameters),
                ComponentType.Hdd => await GetFilteredComponentsAndCount<Hdd>(parameters),
                ComponentType.Fan => await GetFilteredComponentsAndCount<Fan>(parameters),
                _ => throw new ArgumentException("Unsupported component type")
            };

            var mappedItems = mapper.MapAll(componentEntities).ToList();

            return new PagedResponse<object>(mappedItems, totalCount, parameters);
        }

        public async Task<object> GetByIdAsync(Guid id, ComponentType componentType)
        {
            var mapper = _detailsMappers.FirstOrDefault(m => m.ComponentType == componentType);
            if (mapper == null) throw new ArgumentException("Unsupported component type");

            object entity = componentType switch
            {
                ComponentType.Cpu => await _unitOfWork.Repository<Cpu>()
                    .GetFirstOrDefaultAsync(c => c.Id == id),
                ComponentType.Gpu => await _unitOfWork.Repository<Gpu>()
                    .GetFirstOrDefaultAsync(g => g.Id == id, includeProperties: "GpuPowerConnectors"),
                ComponentType.Motherboard => await _unitOfWork.Repository<Motherboard>()
                    .GetFirstOrDefaultAsync(m => m.Id == id, includeProperties: "CpuPowerConnectors,PcleSlots,M2Slots,RearPorts,InnerPorts"),
                ComponentType.Ram => await _unitOfWork.Repository<Ram>()
                    .GetFirstOrDefaultAsync(r => r.Id == id),
                ComponentType.CpuCooler => await _unitOfWork.Repository<CpuCooler>()
                    .GetFirstOrDefaultAsync(c => c.Id == id, includeProperties: "CpuCoolerSockets"),
                ComponentType.PcCase => await _unitOfWork.Repository<PcCase>()
                    .GetFirstOrDefaultAsync(c => c.Id == id, includeProperties: "PcCaseFormFactors,PcCaseFanLocations"),
                ComponentType.PowerSupply => await _unitOfWork.Repository<PowerSupply>()
                    .GetFirstOrDefaultAsync(ps => ps.Id == id, includeProperties: "PowerSupplyPowerConnectors"),
                ComponentType.Ssd => await _unitOfWork.Repository<Ssd>()
                    .GetFirstOrDefaultAsync(s => s.Id == id),
                ComponentType.Hdd => await _unitOfWork.Repository<Hdd>()
                    .GetFirstOrDefaultAsync(h => h.Id == id),
                ComponentType.Fan => await _unitOfWork.Repository<Fan>()
                    .GetFirstOrDefaultAsync(f => f.Id == id),
                _ => throw new ArgumentException("Unsupported component type")
            };

            if (entity == null)
                throw new KeyNotFoundException($"Component with ID {id} not found for type {componentType}");

            var offers = await _unitOfWork.Repository<ProductOffer>().GetAllAsync(p => p.ComponentId == id && p.ComponentType == componentType, includeProperties: "Store");

            return mapper.MapById(entity, offers);
        }

        private async Task<(IEnumerable<object>, int)> GetFilteredComponentsAndCount<T>(
            ResourceParameters parameters,
            string includeProperties = "") where T : class
        {
            var repository = _unitOfWork.Repository<T>();

            var result = await repository.GetFilteredAndPagedAsync(
                parameters: parameters,
                includeProperties: includeProperties
            );

            return (result.items.Cast<object>(), result.totalCount);
        }
    }
}
