using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class PcCase
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

        [MaxLength(100)]
        public string? SizeStandard { get; set; }

        [MaxLength(100)]
        public string? SizeDimentions { get; set; }
        public double? Weight { get; set; }
        public int? PsuWattage { get; set; }

        [MaxLength(100)]
        public string? PsuLocation { get; set; }

        public double? MaxGpuLength { get; set; }
        public double? MaxCpuCoolerHeight { get; set; }
        public bool HasDustFilters { get; set; }

        [MaxLength(200)]
        public string? BuiltInFans { get; set; }

        [MaxLength(400)]
        public string? AdditionalFanPlaces { get; set; }
        public int? Slot25Quant { get; set; }
        public int? Slot35Quant { get; set; }
        public int? Slot525Quant { get; set; }
        public int? ExpansionSlotQuant { get; set; }

        [MaxLength(100)]
        public string? Usb { get; set; }
        public bool HasHeadphones { get; set; }
        public bool HasMicrophone { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        public List<PcCaseFormFactor> PcCaseFormFactors { get; set; } = new();
        public List<PcCaseFanLocation> PcCaseFanLocations { get; set; } = new();
        public List<PcBuild> PcBuilds { get; set; } = new();
    }
}
