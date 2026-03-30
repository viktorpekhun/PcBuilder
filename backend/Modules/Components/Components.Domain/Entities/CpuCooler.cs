using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class CpuCooler
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public LocalizedDescription Description { get; set; } = new();

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        public int? FanCount { get; set; }
        public double? FanSize { get; set; }

        [MaxLength(100)]
        public string? RadiatorMaterial { get; set; }

        [MaxLength(100)]
        public string? SpeedControl { get; set; }

        [MaxLength(100)]
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
