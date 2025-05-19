using AutoMapper;
using PcBuilderApi.Dtos.GpuDtos;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.GpuMappers
{
    public class GpuMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.Gpu;
        public GpuMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var gpu = (Gpu)entity;
            var offers = productOffers.Where(p => p.ComponentId == gpu.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);
            var gpuPowerConnectorsDtos = gpu.GpuPowerConnectors
                .Select(c => new GpuPowerConnectorDto
                {
                    Pins = c.Pins,
                    Quantity = c.Quantity
                })
                .ToList();

            return new GpuDto
            {
                Id = gpu.Id,
                Name = gpu.Name,
                PhotoUrl = gpu.PhotoUrl,
                Description = gpu.Description,
                Brand = gpu.Brand,
                GpuManufacturer = gpu.GpuManufacturer,
                GpuModel = gpu.GpuModel,
                Memory = gpu.Memory,
                MemoryType = gpu.MemoryType,
                PcleVersion = gpu.PcleVersion,
                PcleLane = gpu.PcleLane,
                MaxFrequency = gpu.MaxFrequency,
                CudaCores = gpu.CudaCores,
                MemorySpeed = gpu.MemorySpeed,
                MemoryBus = gpu.MemoryBus,
                SizeLength = gpu.SizeLength,
                SizeWidth = gpu.SizeWidth,
                SizeHeight = gpu.SizeHeight,
                Wattage = gpu.Wattage,
                PsuReccomended = gpu.PsuReccomended,
                FactoryLink = gpu.FactoryLink,
                AveragePrice = gpu.AveragePrice,
                OffersCount = gpu.OffersCount,
                GpuPowerConnectors = gpuPowerConnectorsDtos,
                ProductOffers = offerDtos
            };
        }

    }
}
