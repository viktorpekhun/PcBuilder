namespace PcBuilderApi.Dtos.CpuDtos
{
    public class CpuListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Socket { get; set; } = string.Empty;
        public double? BasicFrequency { get; set; }
        public double? MaxFrequency { get; set; }
        public int? Cores { get; set; }
        public int? Threads { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
