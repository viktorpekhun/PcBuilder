using AutoMapper;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;
using PcBuilderApi.Dtos.CpuCoolerDtos;

namespace PcBuilderApi.Mappers.CpuCoolerMappers
{
    public class CpuCoolerMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.CpuCooler;
        public CpuCoolerMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var cpuCooler = (CpuCooler)entity;
            var offers = productOffers.Where(p => p.ComponentId == cpuCooler.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);
            var cpuCoolerSocketDtos = cpuCooler.CpuCoolerSockets
                .Select(c => new CpuCoolerSocketDto
                {
                    SocketType = c.SocketType
                })
                .ToList();

            return new CpuCoolerDto
            {
                Id = cpuCooler.Id,
                Name = cpuCooler.Name,
                PhotoUrl = cpuCooler.PhotoUrl,
                Description = cpuCooler.Description,
                Brand = cpuCooler.Brand,
                Type = cpuCooler.Type,
                FanCount = cpuCooler.FanCount,
                FanSize = cpuCooler.FanSize,
                RadiatorMaterial = cpuCooler.RadiatorMaterial,
                SpeedControl = cpuCooler.SpeedControl,
                PowerConnector = cpuCooler.PowerConnector,
                MaxPowerDissipation = cpuCooler.MaxPowerDissipation,
                MaxSpeed = cpuCooler.MaxSpeed,
                MinSpeed = cpuCooler.MinSpeed,
                AirflowCfm = cpuCooler.AirflowCfm,
                NoiseLevelDb = cpuCooler.NoiseLevelDb,
                Voltage = cpuCooler.Voltage,
                Lifespan = cpuCooler.Lifespan,
                Length = cpuCooler.Length,
                Width = cpuCooler.Width,
                Height = cpuCooler.Height,
                Weight = cpuCooler.Weight,
                Wattage = cpuCooler.Wattage,
                FactoryLink = cpuCooler.FactoryLink,
                CpuCoolerSockets = cpuCoolerSocketDtos,
                ProductOffers = offerDtos
            };
        }
    }
}
