using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Cpu
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Socket { get; set; } = string.Empty;

        [Range(0.1, 10.0)]
        public double? BasicFrequency { get; set; }

        [Range(0.1, 10.0)]
        public double? MaxFrequency { get; set; }

        [Range(1, 128)]
        public int? Cache { get; set; }

        public string? DimmType { get; set; }

        [Range(1, 128)]
        public int? Cores { get; set; }

        [Range(1, 256)]
        public int? Threads { get; set; }

        public string? Techprocess { get; set; }

        public int? Tdp { get; set; }

        public bool IntegratedGraphics { get; set; }

        public void SetIntegratedGraphics(bool? hasIntegratedGraphics)
        {
            IntegratedGraphics = hasIntegratedGraphics ?? false;
        }

        public string? Complectation { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        public decimal? AveragePrice { get; set; }

        public int? OffersCount { get; set; }
    }
}
