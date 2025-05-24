using AutoMapper;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;
using PcBuilderApi.Dtos.PcCaseDtos;

namespace PcBuilderApi.Mappers.PcCaseMappers
{
    public class PcCaseMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.PcCase;
        public PcCaseMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var pcCase = (PcCase)entity;
            var offers = productOffers.Where(p => p.ComponentId == pcCase.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);
            var pcCaseFormFactorDtos = pcCase.PcCaseFormFactors
                .Select(c => new PcCaseFormFactorDto
                {
                    Name = c.Name
                })
                .ToList();
            var pcCaseFanLocationDtos = pcCase.PcCaseFanLocations
                .Select(c => new PcCaseFanLocationDto
                {
                    Name = c.Name,
                    FanSize = c.FanSize,
                    MaxFans = c.MaxFans
                })
                .ToList();

            return new PcCaseDto
            {
                Id = pcCase.Id,
                Name = pcCase.Name,
                PhotoUrl = pcCase.PhotoUrl,
                Description = pcCase.Description,
                Brand = pcCase.Brand,
                SizeStandard = pcCase.SizeStandard,
                SizeDimentions = pcCase.SizeDimentions,
                Weight = pcCase.Weight,
                PsuLocation = pcCase.PsuLocation,
                MaxGpuLength = pcCase.MaxGpuLength,
                MaxCpuCoolerHeight = pcCase.MaxCpuCoolerHeight,
                HasDustFilters = pcCase.HasDustFilters,
                BuiltInFans = pcCase.BuiltInFans,
                AdditionalFanPlaces = pcCase.AdditionalFanPlaces,
                Slot25Quant = pcCase.Slot25Quant,
                Slot35Quant = pcCase.Slot35Quant,
                Slot525Quant = pcCase.Slot525Quant,
                ExpansionSlotQuant = pcCase.ExpansionSlotQuant,
                Usb = pcCase.Usb,
                HasHeadphones = pcCase.HasHeadphones,
                HasMicrophone = pcCase.HasMicrophone,
                FactoryLink = pcCase.FactoryLink,
                AveragePrice = pcCase.AveragePrice,
                OffersCount = pcCase.OffersCount,
                PcCaseFormFactors = pcCaseFormFactorDtos,
                PcCaseFanLocations = pcCaseFanLocationDtos,
                ProductOffers = offerDtos
            };
        }

    }
}
