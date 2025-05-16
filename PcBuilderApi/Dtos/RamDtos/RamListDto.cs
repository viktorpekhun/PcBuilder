namespace PcBuilderApi.Dtos.RamDtos
{
    public class RamListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public int? Capacity { get; set; }
        public string? Timings { get; set; }
        public double? Voltage { get; set; }
        public bool Xmp { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
