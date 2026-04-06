using Components.Application.Dtos;
﻿
namespace Components.Application.Dtos.PcCaseDtos
{
    public class PcCaseDto : IComponentDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string? SizeStandard { get; set; }
        public string? SizeDimentions { get; set; }
        public double? Weight { get; set; }
        public LocalizedStringDto? PsuLocation { get; set; }
        public double? MaxGpuLength { get; set; }
        public double? MaxCpuCoolerHeight { get; set; }
        public bool HasDustFilters { get; set; }
        public LocalizedStringDto? BuiltInFans { get; set; }
        public LocalizedStringDto? AdditionalFanPlaces { get; set; }
        public int? Slot25Quant { get; set; }
        public int? Slot35Quant { get; set; }
        public int? Slot525Quant { get; set; }
        public int? ExpansionSlotQuant { get; set; }
        public string? Usb { get; set; }
        public bool HasHeadphones { get; set; }
        public bool HasMicrophone { get; set; }
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
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
        public LocalizedStringDto Name { get; set; } = new();
        public int FanSize { get; set; }
        public int MaxFans { get; set; }
    }
}
