namespace Components.Application.Dtos.PowerSupplyDtos
{
    public class PowerSupplyListDto : IComponentListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string FormFactor { get; set; } = string.Empty;
        public int Wattage { get; set; }
        public string? EfficiencyStandart { get; set; }
        public double? EfficiencyPercent { get; set; }
        public double? NoiseLevelMaxDb { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
        public List<PowerSupplyCpuPowerConnectorDto> PowerSupplyCpuPowerConnectors { get; set; } = new();
        public List<PowerSupplyGpuPowerConnectorDto> PowerSupplyGpuPowerConnectors { get; set; } = new();
    }
}
