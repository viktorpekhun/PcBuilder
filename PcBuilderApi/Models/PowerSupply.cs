using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
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

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string FormFactor { get; set; } = string.Empty;

        [Required]
        public int Wattage { get; set; }
        public int? MolexCount { get; set; }
        public int? SataCount { get; set; }
        public int? FddCount { get; set; }
        public int? InputMinVoltage { get; set; }
        public int? InputMaxVoltage { get; set; }
        public bool HasApcf { get; set; }

        [MaxLength(20)]
        public string? EfficiencyStandart { get; set; }
        public double? EfficiencyPercent { get; set; }
        public bool? IsModular { get; set; }
        public int? NoiseLevelMaxDb { get; set; }

        [MaxLength(30)]
        public string? Size { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        public List<PowerSupplyPowerConnector> PowerSupplyPowerConnectors { get; set; } = new();

    }
}
