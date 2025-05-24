using PcBuilderApi.Dtos.PcCaseDtos;
using PcBuilderApi.Dtos.PowerSupplyDtos;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers.PowerSupplyMappers
{
    public class PowerSupplyListMapper : IComponentListMapper
    {
        public ComponentType ComponentType => ComponentType.PowerSupply;
        public IEnumerable<object> MapAll(IEnumerable<object> entities)
        {
            var powerSupplies = entities.Cast<PowerSupply>();
            return powerSupplies.Select(powerSupply =>
            {

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
                    NoiseLevelMaxDb = powerSupply.NoiseLevelMaxDb,
                    AveragePrice = powerSupply.AveragePrice,
                    OffersCount = powerSupply.OffersCount,
                    PowerSupplyCpuPowerConnectors = powerSupplyCpuPowerConnectorDtos,
                    PowerSupplyGpuPowerConnectors = powerSupplyGpuPowerConnectorDtos
                };
            });
        }
    }
}
