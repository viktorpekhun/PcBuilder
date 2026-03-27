using Components.Application.Dtos;

namespace Components.Application.Dtos.GpuDtos
{
    public class GpuDto : IComponentDetailDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Description { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string GpuManufacturer { get; set; } = string.Empty;
        public string GpuModel { get; set; } = string.Empty;
        public int? Memory { get; set; }
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
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<GpuPowerConnectorDto> GpuPowerConnectors { get; set; } = new();

        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }

    public class GpuPowerConnectorDto
    {
        public int Pins { get; set; }

        public int Quantity { get; set; }
    }
}
