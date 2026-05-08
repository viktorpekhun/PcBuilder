using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Gpu : IHasAveragePrice
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string GpuManufacturer { get; set; } = string.Empty;

        public string GpuModel { get; set; } = string.Empty;

        public int? Memory { get; set; }

        public string? MemoryType { get; set; }

        public double? PcleVersion { get; set; }

        public int? PcleLane { get; set; }

        public int? MaxFrequency { get; set; }

        public int? CudaCores { get; set; }

        public int? MemorySpeed { get; set; }

        public int? MemoryBus { get; set; }

        public double? SizeLength { get; set; }

        public double? SizeWidth { get; set; }

        public double? SizeHeight { get; set; }

        public int? Wattage { get; set; }

        public int? PsuReccomended { get; set; }

        /// <summary>
        /// PassMark GPU score. <b>0 indicates missing data</b> — consumers MUST treat
        /// this as "unknown" and skip bottleneck/scoring logic for that component to
        /// avoid DivideByZero and false-negative warnings.
        /// </summary>
        public int PassMarkScore { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        [Url]
        public string? HotlineUrl { get; set; }

        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<GpuPowerConnector> GpuPowerConnectors { get; set; } = new();
    }
}
