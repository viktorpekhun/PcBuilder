using AutoMapper;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;
using PcBuilderApi.Dtos.FanDtos;

namespace PcBuilderApi.Mappers.FanMappers
{
    public class FanMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.Fan;
        public FanMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var fan = (Fan)entity;
            var offers = productOffers.Where(p => p.ComponentId == fan.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);

            return new FanDto
            {
                Id = fan.Id,
                Name = fan.Name,
                PhotoUrl = fan.PhotoUrl,
                Description = fan.Description,
                Brand = fan.Brand,
                ModuleCount = fan.ModuleCount,
                BearingType = fan.BearingType,
                SpeedControl = fan.SpeedControl,
                Connector = fan.Connector,
                Color = fan.Color,
                MinSpeed = fan.MinSpeed,
                MaxSpeed = fan.MaxSpeed,
                AirflowCfm = fan.AirflowCfm,
                NoiseLevelDb = fan.NoiseLevelDb,
                Voltage = fan.Voltage,
                SizeLength = fan.SizeLength,
                SizeWidth = fan.SizeWidth,
                SizeHeight = fan.SizeHeight,
                Weight = fan.Weight,
                Wattage = fan.Wattage,
                FactoryLink = fan.FactoryLink,
                ProductOffers = offerDtos
            };
        }
    }
}
