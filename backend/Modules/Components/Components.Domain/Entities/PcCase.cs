using System.ComponentModel.DataAnnotations;
using Components.Domain.ValueObjects;

namespace Components.Domain.Entities
{
    public class PcCase
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string? SizeStandard { get; set; }

        public string? SizeDimentions { get; set; }
        public double? Weight { get; set; }
        public int? PsuWattage { get; set; }

        public LocalizedString? PsuLocation { get; set; }

        public double? MaxGpuLength { get; set; }
        public double? MaxCpuCoolerHeight { get; set; }
        public bool HasDustFilters { get; set; }

        public LocalizedString? BuiltInFans { get; set; }

        public LocalizedString? AdditionalFanPlaces { get; set; }
        public int? Slot25Quant { get; set; }
        public int? Slot35Quant { get; set; }
        public int? Slot525Quant { get; set; }
        public int? ExpansionSlotQuant { get; set; }

        public string? Usb { get; set; }
        public bool HasHeadphones { get; set; }
        public bool HasMicrophone { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
        public List<PcCaseFormFactor> PcCaseFormFactors { get; set; } = new();
        public List<PcCaseFanLocation> PcCaseFanLocations { get; set; } = new();
    }
}
