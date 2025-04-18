using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
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

        [MaxLength(3000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        [MaxLength(30)]
        public string? Interface { get; set; }
        public double? FormFactor { get; set; }
        public int? SpindleSpeed { get; set; }
        public int? Cache { get; set; }
        public int? Speed { get; set; }

        [MaxLength(20)]
        public string? WritingTechnology { get; set; }
        public int? AverageLifetime { get; set; }
        public int? NoiceDb { get; set; }
        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

    }
}
