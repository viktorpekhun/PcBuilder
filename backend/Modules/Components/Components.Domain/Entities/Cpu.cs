using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Cpu
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
        [MaxLength(50)]
        public string Socket { get; set; } = string.Empty;

        [Range(0.1, 10.0)]
        public double? BasicFrequency { get; set; }

        [Range(0.1, 10.0)]
        public double? MaxFrequency { get; set; }

        [Range(1, 128)]
        public int? Cache { get; set; }

        [MaxLength(50)]
        public string? DimmType { get; set; }

        [Range(1, 128)]
        public int? Cores { get; set; }

        [Range(1, 256)]
        public int? Threads { get; set; }

        [MaxLength(50)]
        public string? Techprocess { get; set; }

        public int? Tdp { get; set; }

        public bool IntegratedGraphics { get; set; }

        public void SetIntegratedGraphics(bool? hasIntegratedGraphics)
        {
            IntegratedGraphics = hasIntegratedGraphics ?? false;
        }

        [MaxLength(255)]
        public string? Complectation { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        public decimal? AveragePrice { get; set; }

        public int? OffersCount { get; set; }
    }
}
