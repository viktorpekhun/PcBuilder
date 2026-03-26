namespace Components.Application.Dtos.GpuDtos
{
    public class GpuListDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string GpuManufacturer { get; set; } = string.Empty;
        public string GpuModel { get; set; } = string.Empty;

        public int? Memory { get; set; }

        public string? MemoryType { get; set; }

        public int? MemoryBus { get; set; }

        public double? PcleVersion { get; set; }

        public int? PcleLane { get; set; }

        public int? MaxFrequency { get; set; }

        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
