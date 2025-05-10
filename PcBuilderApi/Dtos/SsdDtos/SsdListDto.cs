namespace PcBuilderApi.Dtos.SsdDtos
{
    public class SsdListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Interface { get; set; }
        public string? NandType { get; set; }
        public bool IsTrimmSupported { get; set; }
        public string? FormFactor { get; set; }
        public int? MaxReadSpeed { get; set; }
        public int? MaxWriteSpeed { get; set; }
        public decimal? AveragePrice { get; set; }
    }
}
