using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class PowerSupply
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
        [MaxLength(50)]
        public string FormFactor { get; set; } = string.Empty;

        [Required]
        public int Wattage { get; set; }
        public int? MolexCount { get; set; }
        public int? SataCount { get; set; }
        public int? FddCount { get; set; }
        public int? InputMinVoltage { get; set; }
        public int? InputMaxVoltage { get; set; }
        public bool HasApcf { get; set; }

        [MaxLength(50)]
        public string? EfficiencyStandart { get; set; }
        public double? EfficiencyPercent { get; set; }
        public string Modularity { get; set; } = string.Empty;
        public double? NoiseLevelMaxDb { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        [Url]
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
        public List<PowerSupplyPowerConnector> PowerSupplyPowerConnectors { get; set; } = new();
    }
}
