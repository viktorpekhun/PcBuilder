using Components.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class CpuCooler
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int? FanCount { get; set; }
        public double? FanSize { get; set; }

        public LocalizedString? RadiatorMaterial { get; set; }

        public string? SpeedControl { get; set; }

        public string? PowerConnector { get; set; }

        public int? MaxPowerDissipation { get; set; }
        public int? MaxSpeed { get; set; }
        public int? MinSpeed { get; set; }
        public double? AirflowCfm { get; set; }
        public double? NoiseLevelDb { get; set; }
        public int? Voltage { get; set; }
        public int? Lifespan { get; set; }
        public double? Length { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<CpuCoolerSocket> CpuCoolerSockets { get; set; } = new();
    }
}
