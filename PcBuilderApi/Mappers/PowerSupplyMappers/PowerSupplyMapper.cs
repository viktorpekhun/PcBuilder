using AutoMapper;
using PcBuilderApi.Dtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;
using PcBuilderApi.Dtos.PowerSupplyDtos;

namespace PcBuilderApi.Mappers.PowerSupplyMappers
{
    public class PowerSupplyMapper : IComponentMapper
    {
        private readonly IMapper _mapper;
        public ComponentType ComponentType => ComponentType.PowerSupply;
        public PowerSupplyMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public object MapById(object entity, IEnumerable<ProductOffer> productOffers)
        {
            var powerSupply = (PowerSupply)entity;
            var offers = productOffers.Where(p => p.ComponentId == powerSupply.Id);
            var offerDtos = _mapper.Map<List<ProductOfferDto>>(offers);
            var powerSupplyMotherboardPowerConnectorDtos = powerSupply.PowerSupplyPowerConnectors
                .Where(c => c.Type == "Motherboard")
                .Select(c => new PowerSupplyMotherboardPowerConnectorDto
                {
                    Pins = c.Pins,
                    AdditionalPins = c.AdditionalPins,
                })
                .ToList();
            var powerSupplyCpuPowerConnectorDtos = powerSupply.PowerSupplyPowerConnectors
                .Where(c => c.Type == "CPU")
                .Select(c => new PowerSupplyCpuPowerConnectorDto
                {
                    Pins = c.Pins,
                    AdditionalPins = c.AdditionalPins,
                    Quantity = c.Quantity
                })
                .ToList();

            var powerSupplyGpuPowerConnectorDtos = powerSupply.PowerSupplyPowerConnectors
                .Where(c => c.Type == "GPU")
                .Select(c => new PowerSupplyGpuPowerConnectorDto
                {
                    Pins = c.Pins,
                    AdditionalPins = c.AdditionalPins,
                    Quantity = c.Quantity
                })
                .ToList();


            return new PowerSupplyDto
            {
                Id = powerSupply.Id,
                Name = powerSupply.Name,
                PhotoUrl = powerSupply.PhotoUrl,
                Description = powerSupply.Description,
                Brand = powerSupply.Brand,
                FormFactor = powerSupply.FormFactor,
                Wattage = powerSupply.Wattage,
                MolexCount = powerSupply.MolexCount,
                SataCount = powerSupply.SataCount,
                FddCount = powerSupply.FddCount,
                InputMinVoltage = powerSupply.InputMinVoltage,
                InputMaxVoltage = powerSupply.InputMaxVoltage,
                HasApcf = powerSupply.HasApcf,
                EfficiencyStandart = powerSupply.EfficiencyStandart,
                EfficiencyPercent = powerSupply.EfficiencyPercent,
                IsModular = powerSupply.IsModular,
                NoiseLevelMaxDb = powerSupply.NoiseLevelMaxDb,
                Size = powerSupply.Size,
                FactoryLink = powerSupply.FactoryLink,
                AveragePrice = powerSupply.AveragePrice,
                OffersCount = powerSupply.OffersCount,
                PowerSupplyMotherboardPowerConnectors = powerSupplyMotherboardPowerConnectorDtos,
                PowerSupplyCpuPowerConnectors = powerSupplyCpuPowerConnectorDtos,
                PowerSupplyGpuPowerConnectors = powerSupplyGpuPowerConnectorDtos,
                ProductOffers = offerDtos
            };
        }

    }
}
