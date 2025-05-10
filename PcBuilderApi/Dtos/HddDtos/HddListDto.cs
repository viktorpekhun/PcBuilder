namespace PcBuilderApi.Dtos.HddDtos
{
    public class HddListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string? Interface { get; set; }
        public string? FormFactor { get; set; }
        public int? SpindleSpeed { get; set; }
        public int? Cache { get; set; }
        public decimal? AveragePrice{ get; set; }
    }
}
