using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class Gpu
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
        [MaxLength(100)]
        public string GpuManufacturer { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GpuModel { get; set; } = string.Empty;

        public int? Memory { get; set; }

        [MaxLength(20)]
        public string? MemoryType { get; set; }

        public double? PcleVersion { get; set; }

        public int? PcleLane { get; set; }

        public int? MaxFrequency { get; set; }

        public int? CudaCores { get; set; }

        public int? MemorySpeed { get; set; }

        public int? MemoryBus { get; set; }

        public double? SizeLength { get; set; }

        public double? SizeWidth { get; set; }

        public double? SizeHeight { get; set; }

        public int? Wattage { get; set; }

        public int? PsuReccomended { get; set; }

        [Url]
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<GpuPowerConnector> GpuPowerConnectors { get; set; } = new();

        public List<PcBuild> PcBuilds { get; set; } = new();
    }
}
