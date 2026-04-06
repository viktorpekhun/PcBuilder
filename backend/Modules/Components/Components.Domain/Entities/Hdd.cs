using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Hdd
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [MaxLength(50)]
        public string? Interface { get; set; }
        public string? FormFactor { get; set; }
        public int? SpindleSpeed { get; set; }
        public int? Cache { get; set; }
        public int? Speed { get; set; }

        [MaxLength(50)]
        public string? WritingTechnology { get; set; }
        public int? NoiceDb { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
    }
}
