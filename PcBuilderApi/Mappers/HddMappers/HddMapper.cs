using AutoMapper;
using PcBuilderApi.Dtos.HddDtos;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.HddMappers
{
    public class HddMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.Hdd;
        public HddMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var hdd = (Hdd)entity;
            var offers = productOffers.Where(p => p.ComponentId == hdd.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);

            return new HddDto
            {
                Id = hdd.Id,
                Name = hdd.Name,
                PhotoUrl = hdd.PhotoUrl,
                Description = hdd.Description,
                Brand = hdd.Brand,
                Capacity = hdd.Capacity,
                Interface = hdd.Interface,
                FormFactor = hdd.FormFactor,
                SpindleSpeed = hdd.SpindleSpeed,
                Cache = hdd.Cache,
                Speed = hdd.Speed,
                WritingTechnology = hdd.WritingTechnology,
                NoiceDb = hdd.NoiceDb,
                Wattage = hdd.Wattage,
                FactoryLink = hdd.FactoryLink,
                AveragePrice = hdd.AveragePrice,
                OffersCount = hdd.OffersCount,
                ProductOffers = offerDtos
            };
        }
    }
}
