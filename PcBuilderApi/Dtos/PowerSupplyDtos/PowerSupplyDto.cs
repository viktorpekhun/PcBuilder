
namespace PcBuilderApi.Dtos.PowerSupplyDtos
{
    public class PowerSupplyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Description { get; set; }
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
        public bool? IsModular { get; set; }
        public double? NoiseLevelMaxDb { get; set; }
        public string? Size { get; set; }
        public string? FactoryLink { get; set; }

        public List<PowerSupplyPowerConnectorDto> PowerSupplyPowerConnectors { get; set; } = new();
        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }
    public class PowerSupplyPowerConnectorDto
    {
        public string Type { get; set; } = string.Empty;
        public int Pins { get; set; }
        public int? AdditionalPins { get; set; }
        public int Quantity { get; set; }
    }
}
