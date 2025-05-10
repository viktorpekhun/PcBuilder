
namespace PcBuilderApi.Dtos.PcCaseDtos
{
    public class PcCaseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Description { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string? SizeStandard { get; set; }
        public string? SizeDimentions { get; set; }
        public double? Weight { get; set; }
        public int? PsuWattage { get; set; }
        public string? PsuLocation { get; set; }
        public double? MaxGpuLength { get; set; }
        public double? MaxCpuCoolerHeight { get; set; }
        public bool HasDustFilters { get; set; }
        public string? BuiltInFans { get; set; }
        public string? AdditionalFanPlaces { get; set; }
        public int? Slot25Quant { get; set; }
        public int? Slot35Quant { get; set; }
        public int? Slot525Quant { get; set; }
        public int? ExpansionSlotQuant { get; set; }
        public string? Usb { get; set; }
        public bool HasHeadphones { get; set; }
        public bool HasMicrophone { get; set; }
        public string? FactoryLink { get; set; }

        public List<PcCaseFormFactorDto> PcCaseFormFactors { get; set; } = new();
        public List<PcCaseFanLocationDto> PcCaseFanLocations { get; set; } = new();
        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }
    public class PcCaseFormFactorDto
    {
        public string Name { get; set; } = string.Empty;
    }
    public class PcCaseFanLocationDto
    {
        public string Name { get; set; } = string.Empty;
        public int FanSize { get; set; }
        public int MaxFans { get; set; }
    }
}
