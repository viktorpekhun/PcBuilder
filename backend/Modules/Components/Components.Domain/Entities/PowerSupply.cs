using Components.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class PowerSupply
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
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

        [Url]
        public string? FactoryLink { get; set; }

        [Url]
        public string? HotlineUrl { get; set; }

        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
        public List<PowerSupplyPowerConnector> PowerSupplyPowerConnectors { get; set; } = new();
    }
}
