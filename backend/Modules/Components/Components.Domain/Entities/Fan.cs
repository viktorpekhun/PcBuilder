using Components.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Fan : IHasAveragePrice
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public int? ModuleCount { get; set; }

        public LocalizedString? BearingType { get; set; }

        public string? SpeedControl { get; set; }

        public string? Connector { get; set; }

        public LocalizedString? Color { get; set; }

        public int? MinSpeed { get; set; }
        public int? MaxSpeed { get; set; }
        public double? AirflowCfm { get; set; }
        public double? NoiseLevelDb { get; set; }
        public int? Voltage { get; set; }
        public double? SizeLength { get; set; }
        public double? SizeWidth { get; set; }
        public double? SizeHeight { get; set; }
        public double? Weight { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        [Url]
        public string? HotlineUrl { get; set; }

        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
