using AutoMapper;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;
using PcBuilderApi.Dtos.RamDtos;

namespace PcBuilderApi.Mappers.RamMappers
{
    public class RamMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.Ram;
        public RamMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var ram = (Ram)entity;
            var offers = productOffers.Where(p => p.ComponentId == ram.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);

            return new RamDto
            {
                Id = ram.Id,
                Name = ram.Name,
                PhotoUrl = ram.PhotoUrl,
                Description = ram.Description,
                Brand = ram.Brand,
                Type = ram.Type,
                Frequency = ram.Frequency,
                Capacity = ram.Capacity,
                ModuleQuantity = ram.ModuleQuantity,
                Timings = ram.Timings,
                Voltage = ram.Voltage,
                Xmp = ram.Xmp,
                Ecc = ram.Ecc,
                Expo = ram.Expo,
                Bufferization = ram.Bufferization,
                Color = ram.Color,
                Wattage = ram.Wattage,
                FactoryLink = ram.FactoryLink,
                AveragePrice = ram.AveragePrice,
                OffersCount = ram.OffersCount,
                ProductOffers = offerDtos
            };
        }
    }
}
