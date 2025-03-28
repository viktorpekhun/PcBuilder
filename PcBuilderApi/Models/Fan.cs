using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class Fan
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        public int? ModuleCount { get; set; }

        [MaxLength(50)]
        public string? BearingType { get; set; }

        [MaxLength(30)]
        public string? SpeedControl { get; set; }

        [MaxLength(20)]
        public string? Connector { get; set; }

        [MaxLength(30)]
        public string? Color { get; set; }

        public int? MinSpeed { get; set; }
        public int? MaxSpeed { get; set; }
        public double? AirFlowCfm { get; set; }
        public double? NoiseDb { get; set; }
        public int? Voltage { get; set; }
        public double? Length { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }
    }
}
