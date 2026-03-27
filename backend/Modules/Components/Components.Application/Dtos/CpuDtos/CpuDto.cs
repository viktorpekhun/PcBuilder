using Components.Application.Dtos;
﻿namespace Components.Application.Dtos.CpuDtos
{
    public class CpuDto : IComponentDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Description { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Socket { get; set; } = string.Empty;
        public double? BasicFrequency { get; set; }
        public double? MaxFrequency { get; set; }
        public int? Cache { get; set; }
        public string? DimmType { get; set; }
        public int? Cores { get; set; }
        public int? Threads { get; set; }
        public string? Techprocess { get; set; }
        public int? Tdp { get; set; }
        public bool IntegratedGraphics { get; set; }
        public string? Complectation { get; set; }
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }
        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }
}
