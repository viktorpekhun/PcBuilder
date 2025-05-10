namespace PcBuilderApi.Dtos.MotherboardDtos
{
    public class MotherboardListDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Socket { get; set; } = string.Empty;

        public string Chipset { get; set; } = string.Empty;

        public int? DimmSlots { get; set; }

        public string? DimmType { get; set; }
        public int? DimmFrequency { get; set; }
        public int? DimmCapacity { get; set; }
        public string? FormFactor { get; set; }
        public string? SizeDimentions { get; set; }
        public decimal? AveragePrice { get; set; }
        public List<PcleSlotDto> PcleSlots { get; set; } = new();
    }
}
