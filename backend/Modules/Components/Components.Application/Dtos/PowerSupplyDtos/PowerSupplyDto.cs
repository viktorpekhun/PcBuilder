using Components.Application.Dtos;
using Components.Domain.ValueObjects;

namespace Components.Application.Dtos.PowerSupplyDtos
{
    public class PowerSupplyDto : IComponentDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string FormFactor { get; set; } = string.Empty;
        public int Wattage { get; set; }
        public int? MolexCount { get; set; }
        public int? SataCount { get; set; }
        public int? FddCount { get; set; }
        public int? InputMinVoltage { get; set; }
        public int? InputMaxVoltage { get; set; }
        public bool HasApcf { get; set; }
        public string? EfficiencyStandart { get; set; }
        public double? EfficiencyPercent { get; set; }
        public LocalizedString? Modularity { get; set; }
        public double? NoiseLevelMaxDb { get; set; }
        public string? Size { get; set; }
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
        public List<PowerSupplyMotherboardPowerConnectorDto> PowerSupplyMotherboardPowerConnectors { get; set; } = new();
        public List<PowerSupplyCpuPowerConnectorDto> PowerSupplyCpuPowerConnectors { get; set; } = new();
        public List<PowerSupplyGpuPowerConnectorDto> PowerSupplyGpuPowerConnectors { get; set; } = new();
        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }
    public class PowerSupplyMotherboardPowerConnectorDto
    {
        public int Pins { get; set; }
        public int Quantity { get; set; }
    }
    public class PowerSupplyCpuPowerConnectorDto
    {
        public int Pins { get; set; }
        public int? AdditionalPins { get; set; }
        public int Quantity { get; set; }
    }
    public class PowerSupplyGpuPowerConnectorDto
    {
        public int Pins { get; set; }
        public int? AdditionalPins { get; set; }
        public int Quantity { get; set; }
    }
}
