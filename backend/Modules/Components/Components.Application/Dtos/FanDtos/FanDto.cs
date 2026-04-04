using Components.Application.Dtos;
﻿using System.ComponentModel.DataAnnotations;

namespace Components.Application.Dtos.FanDtos
{
    public class FanDto : IComponentDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string Brand { get; set; } = string.Empty;
        public int? ModuleCount { get; set; }
        public string? BearingType { get; set; }
        public string? SpeedControl { get; set; }
        public string? Connector { get; set; }
        public string? Color { get; set; }
        public int? MinSpeed { get; set; }
        public int? MaxSpeed { get; set; }
        public double? AirflowCfm { get; set; }
        public double? NoiseLevelDb { get; set; }
        public int? Voltage { get; set; }
        public double? SizeLength { get; set; }
        public double? SizeWidth { get; set; }
        public double? SizeHeight { get; set; }
        public double? Weight { get; set; }
        public int? Wattage { get; set; }
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }
}
