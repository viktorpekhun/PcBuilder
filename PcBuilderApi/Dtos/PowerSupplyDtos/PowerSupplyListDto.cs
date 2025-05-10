namespace PcBuilderApi.Dtos.PowerSupplyDtos
{
    public class PowerSupplyListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string FormFactor { get; set; } = string.Empty;
        public int Wattage { get; set; }
        public string? EfficiencyStandart { get; set; }
        public double? EfficiencyPercent { get; set; }
        public bool? IsModular { get; set; }
        public double? NoiseLevelMaxDb { get; set; }
        public decimal? AveragePrice { get; set; }
        public List<PowerSupplyPowerConnectorDto> PowerSupplyPowerConnectors { get; set; } = new();
    }
}
