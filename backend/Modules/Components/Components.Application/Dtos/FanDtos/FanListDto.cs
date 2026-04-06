using Components.Domain.ValueObjects;

namespace Components.Application.Dtos.FanDtos
{
    public class FanListDto : IComponentListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public int? ModuleCount { get; set; }
        public LocalizedString? BearingType { get; set; }
        public string? SpeedControl { get; set; }
        public string? Connector { get; set; }
        public LocalizedString? Color { get; set; }
        public int? MaxSpeed { get; set; }
        public double? NoiseLevelDb { get; set; }
        public double? SizeLength { get; set; }
        public double? SizeWidth { get; set; }
        public double? SizeHeight { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
