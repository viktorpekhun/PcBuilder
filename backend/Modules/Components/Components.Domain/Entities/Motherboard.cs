using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class Motherboard
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

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

        [Url]
        public string? FactoryLink { get; set; }
        public decimal? AveragePrice { get; set; }
        public int? OffersCount { get; set; }

        public List<CpuPowerConnector> CpuPowerConnectors { get; set; } = new();
        public List<PcleSlot> PcleSlots { get; set; } = new();
        public List<M2Slot> M2Slots { get; set; } = new();
        public List<RearPort> RearPorts { get; set; } = new();
        public List<InnerPort> InnerPorts { get; set; } = new();
    }
}
