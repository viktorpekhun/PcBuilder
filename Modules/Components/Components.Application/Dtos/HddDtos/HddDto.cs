using Components.Application.Dtos;
﻿
namespace Components.Application.Dtos.HddDtos
{
    public class HddDto : IHasProductOffers
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Description { get; set; }
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
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }
}
