using PcBuilderApi.Models;
using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Dtos.CpuCoolerDtos
{
    public class CpuCoolerListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double? FanSize { get; set; }
        public string? RadiatorMaterial { get; set; }
        public string? SpeedControl { get; set; }
        public string? PowerConnector { get; set; }
        public int? MaxPowerDissipation { get; set; }
        public int? MaxSpeed { get; set; }
        public double? NoiseLevelDb { get; set; }
        public double? Length { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<CpuCoolerSocketDto> CpuCoolerSockets { get; set; } = new();
    }

}
