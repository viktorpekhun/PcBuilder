using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Hdd
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public string? Interface { get; set; }
        public string? FormFactor { get; set; }
        public int? SpindleSpeed { get; set; }
        public int? Cache { get; set; }
        public int? Speed { get; set; }

        public string? WritingTechnology { get; set; }
        public int? NoiceDb { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        [Url]
        public string? HotlineUrl { get; set; }

        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
