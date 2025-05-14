using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class Ssd
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
        public int Capacity { get; set; }

        [MaxLength(300)]
        public string? Interface { get; set; }

        [MaxLength(50)]
        public string? NandType { get; set; }
        public bool IsTrimmSupported { get; set; }

        [MaxLength(50)]
        public string? FormFactor { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        public double? Weight { get; set; }
        public int? MaxReadSpeed { get; set; }
        public int? MaxWriteSpeed { get; set; }
        public int? RandomReadSpeed { get; set; }
        public int? RandomWriteSpeed { get; set; }
        public int? WritingRecource { get; set; }
        public double? AverageLifeTime { get; set; }

        [Required]
        public int Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<PcBuild_Ssd> PcBuild_Ssds { get; set; } = new();

    }
}
