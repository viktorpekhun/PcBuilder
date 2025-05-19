using AutoMapper;
using PcBuilderApi.Dtos.RamDtos;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;
using PcBuilderApi.Utilities;
using static PcBuilderApi.Utilities.SD;
using PcBuilderApi.Dtos.SsdDtos;

namespace PcBuilderApi.Mappers.SsdMappers
{
    public class SsdMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.Ssd;
        public SsdMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var ssd = (Ssd)entity;
            var offers = productOffers.Where(p => p.ComponentId == ssd.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);

            return new SsdDto
            {
                Id = ssd.Id,
                Name = ssd.Name,
                PhotoUrl = ssd.PhotoUrl,
                Description = ssd.Description,
                Brand = ssd.Brand,
                Capacity = ssd.Capacity,
                Interface = ssd.Interface,
                NandType = ssd.NandType,
                IsTrimmSupported = ssd.IsTrimmSupported,
                FormFactor = ssd.FormFactor,
                Size = ssd.Size,
                Weight = ssd.Weight,
                MaxReadSpeed = ssd.MaxReadSpeed,
                MaxWriteSpeed = ssd.MaxWriteSpeed,
                RandomReadSpeed = ssd.RandomReadSpeed,
                RandomWriteSpeed = ssd.RandomWriteSpeed,
                WritingRecource = ssd.WritingRecource,
                AverageLifeTime = ssd.AverageLifeTime,
                Wattage = ssd.Wattage,
                FactoryLink = ssd.FactoryLink,
                AveragePrice = ssd.AveragePrice,
                OffersCount = ssd.OffersCount,
                ProductOffers = offerDtos
            };
        }
    }
}
