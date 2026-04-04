using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
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
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        public int? ModuleCount { get; set; }

        [MaxLength(100)]
        public string? BearingType { get; set; }

        [MaxLength(50)]
        public string? SpeedControl { get; set; }

        [MaxLength(50)]
        public string? Connector { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        public int? MinSpeed { get; set; }
        public int? MaxSpeed { get; set; }
        public double? AirflowCfm { get; set; }
        public double? NoiseLevelDb { get; set; }
        public int? Voltage { get; set; }
        public double? SizeLength { get; set; }
        public double? SizeWidth { get; set; }
        public double? SizeHeight { get; set; }
        public double? Weight { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
