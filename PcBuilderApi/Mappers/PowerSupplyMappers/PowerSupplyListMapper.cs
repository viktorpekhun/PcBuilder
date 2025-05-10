using PcBuilderApi.Dtos.PcCaseDtos;
using PcBuilderApi.Dtos.PowerSupplyDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.PowerSupplyMappers
{
    public class PowerSupplyListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.PowerSupply;
        public IEnumerable<object> MapAll(IEnumerable<object> entities, IEnumerable<ProductOffer> productOffers)
        {
            var powerSupplys = entities.Cast<PowerSupply>();
            var filteredOffers = productOffers.Where(p => p.ComponentType == ComponentType);
            return powerSupplys.Select(powerSupply =>
            {
                var offers = filteredOffers.Where(p => p.ComponentId == powerSupply.Id);
                var avgPrice = offers.Any() ? offers.Average(p => p.Price) : 0;
                var powerSupplyPowerConnectorDtos = powerSupply.PowerSupplyPowerConnectors
                     .Select(c => new PowerSupplyPowerConnectorDto
                     {
                         Type = c.Type,
                         Pins = c.Pins,
                         AdditionalPins = c.AdditionalPins,
                         Quantity = c.Quantity
                     })
                     .ToList();
                return new PowerSupplyListDto
                {
                    Id = powerSupply.Id,
                    Name = powerSupply.Name,
                    PhotoUrl = powerSupply.PhotoUrl,
                    Brand = powerSupply.Brand,
                    FormFactor = powerSupply.FormFactor,
                    Wattage = powerSupply.Wattage,
                    EfficiencyPercent = powerSupply.EfficiencyPercent,
                    EfficiencyStandart = powerSupply.EfficiencyStandart,
                    IsModular = powerSupply.IsModular,
                    NoiseLevelMaxDb = powerSupply.NoiseLevelMaxDb,
                    AveragePrice = avgPrice,
                    PowerSupplyPowerConnectors = powerSupplyPowerConnectorDtos,
                };
            });
        }
    }
}
