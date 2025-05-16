using PcBuilderApi.Models;

namespace PcBuilderApi.Dtos.MotherboardDtos
{
    public class MotherboardDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public string? Description { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Socket { get; set; } = string.Empty;

        public string Chipset { get; set; } = string.Empty;

        public int? DimmSlots { get; set; }

        public string? DimmType { get; set; }
        public int? DimmFrequency { get; set; }
        public int? DimmCapacity { get; set; }
        public int? Sata3Count { get; set; }
        public int? PowerMotherboard { get; set; }
        public int? FanQuantity { get; set; }
        public int? PcleX1Quantity { get; set; }
        public string? Ethernet { get; set; }

        public string? Audio { get; set; }

        public string? Wifi { get; set; }

        public string? Bluetooth { get; set; }

        public string? VideoPorts { get; set; }

        public string? FormFactor { get; set; }

        public string? SizeDimentions { get; set; }

        public int? Wattage { get; set; }

        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<CpuPowerConnectorDto> CpuPowerConnectors { get; set; } = new();
        public List<PcleSlotDto> PcleSlots { get; set; } = new();
        public List<M2SlotDto> M2Slots { get; set; } = new();
        public List<RearPortDto> RearPorts { get; set; } = new();
        public List<InnerPortDto> InnerPorts { get; set; } = new();
        public List<ProductOfferDto> ProductOffers { get; set; } = new();
    }

    public class CpuPowerConnectorDto
    {
        public int Pins { get; set; }
        public int Quantity { get; set; }
    }

    public class PcleSlotDto
    {
        public double Version { get; set; }
        public int? Lane { get; set; }
        public int? Quantity { get; set; }
    }
    public class M2SlotDto
    {
        public double Version { get; set; }
        public int? Lane { get; set; }
        public int? Quantity { get; set; }
    }
    public class RearPortDto
    {
        public string Type { get; set; } = string.Empty;
        public int? Quantity { get; set; }
    }
    public class InnerPortDto
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}