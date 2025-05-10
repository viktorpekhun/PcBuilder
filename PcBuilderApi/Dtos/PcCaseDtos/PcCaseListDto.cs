namespace PcBuilderApi.Dtos.PcCaseDtos
{
    public class PcCaseListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string? SizeStandard { get; set; }
        public string? SizeDimentions { get; set; }
        public int? PsuWattage { get; set; }
        public string? PsuLocation { get; set; }
        public bool HasDustFilters { get; set; }
        public string? Usb { get; set; }
        public decimal? AveragePrice { get; set; }

        public List<PcCaseFormFactorDto> PcCaseFormFactors { get; set; } = new();
    }
}
