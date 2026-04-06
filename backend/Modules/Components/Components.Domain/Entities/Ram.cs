using Components.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Ram
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int Frequency { get; set; }
        public int? Capacity { get; set; }
        public int? ModuleQuantity { get; set; }

        public string? Timings { get; set; }
        public double? Voltage { get; set; }
        public bool Xmp { get; set; }
        public bool Ecc { get; set; }
        public bool Expo { get; set; }

        public string? Bufferization { get; set; }

        public LocalizedString? Color { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
