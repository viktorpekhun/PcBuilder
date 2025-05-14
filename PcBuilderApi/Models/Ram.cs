using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class Ram
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        [MaxLength(3000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        public int Frequency { get; set; }
        public int? Capacity { get; set; }
        public int? ModuleQuantity { get; set; }

        [MaxLength(50)]
        public string? Timings { get; set; }
        public double? Voltage { get; set; }
        public bool Xmp { get; set; }
        public bool Ecc { get; set; }
        public bool Expo { get; set; }

        [MaxLength(50)]
        public string? Bufferization { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<PcBuild_Ram> PcBuild_Rams { get; set; } = new();
    }
}
