using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Ssd
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public string? Interface { get; set; }

        public string? NandType { get; set; }
        public bool IsTrimmSupported { get; set; }

        public string? FormFactor { get; set; }

        public string? Size { get; set; }

        public double? Weight { get; set; }
        public int? MaxReadSpeed { get; set; }
        public int? MaxWriteSpeed { get; set; }
        public int? RandomReadSpeed { get; set; }
        public int? RandomWriteSpeed { get; set; }
        public int? WritingRecource { get; set; }
        public double? AverageLifeTime { get; set; }

        public int Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
