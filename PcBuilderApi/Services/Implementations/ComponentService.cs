using PcBuilderApi.Mappers;
using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Services.Interfaces;
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

        public async Task<IEnumerable<object>> GetAllByTypeAsync(ComponentType componentType)
        {
            var mapper = _listMappers.FirstOrDefault(m => m.ComponentType == componentType);
            if (mapper == null) throw new ArgumentException("Unsupported component type");

            IEnumerable<object> componentEntities = componentType switch
            {
                ComponentType.Cpu => (await _unitOfWork.Repository<Cpu>()
                    .GetAllAsync()).Cast<object>(),
                ComponentType.Gpu => (await _unitOfWork.Repository<Gpu>()
                    .GetAllAsync(includeProperties: "GpuPowerConnectors")).Cast<object>(),
                ComponentType.Motherboard => (await _unitOfWork.Repository<Motherboard>()
                    .GetAllAsync(includeProperties: "CpuPowerConnectors,PcleSlots,M2Slots,RearPorts,InnerPorts")).Cast<object>(),
                ComponentType.Ram => (await _unitOfWork.Repository<Ram>()
                    .GetAllAsync()).Cast<object>(),
                ComponentType.CpuCooler => (await _unitOfWork.Repository<CpuCooler>()
                    .GetAllAsync(includeProperties: "CpuCoolerSockets")).Cast<object>(),
                ComponentType.PcCase => (await _unitOfWork.Repository<PcCase>()
                    .GetAllAsync(includeProperties: "PcCaseFormFactors,PcCaseFanLocations")).Cast<object>(),
                ComponentType.PowerSupply => (await _unitOfWork.Repository<PowerSupply>()
                    .GetAllAsync(includeProperties: "PowerSupplyPowerConnectors")).Cast<object>(),
                ComponentType.Ssd => (await _unitOfWork.Repository<Ssd>()
                    .GetAllAsync()).Cast<object>(),
                ComponentType.Hdd => (await _unitOfWork.Repository<Hdd>()
                    .GetAllAsync()).Cast<object>(),
                ComponentType.Fan => (await _unitOfWork.Repository<Fan>()
                    .GetAllAsync()).Cast<object>(),
                _ => throw new ArgumentException("Unsupported component type")
            };

            var offers = await _unitOfWork.Repository<ProductOffer>().GetAllAsync(p => p.ComponentType == componentType);

            return mapper.MapAll(componentEntities, offers);
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
    }
}
