using Components.Application.Dtos;
﻿
namespace Components.Application.Dtos.CpuCoolerDtos
{
    public class CpuCoolerDto : IHasProductOffers
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Description { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? FanCount { get; set; }
        public double? FanSize { get; set; }
        public string? RadiatorMaterial { get; set; }
        public string? SpeedControl { get; set; }
        public string? PowerConnector { get; set; }
        public int? MaxPowerDissipation { get; set; }
        public int? MaxSpeed { get; set; }
        public int? MinSpeed { get; set; }
        public double? AirflowCfm { get; set; }
        public double? NoiseLevelDb { get; set; }
        public int? Voltage { get; set; }
        public int? Lifespan { get; set; }
        public double? Length { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public int? Wattage { get; set; }
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<CpuCoolerSocketDto> CpuCoolerSockets { get; set; } = new();
        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }

    public class CpuCoolerSocketDto
    {
        public string SocketType { get; set; } = string.Empty;
    }
}
