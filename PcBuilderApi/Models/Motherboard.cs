using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class Motherboard
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Url]
        public string? PhotoUrl { get; set; }

        [MaxLength(3000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Socket { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Chipset { get; set; } = string.Empty;

        public int? DimmSlots { get; set; }

        [MaxLength(20)]
        public string? DimmType { get; set; }
        public int? DimmFrequency { get; set; }
        public int? DimmCapacity { get; set; }
        public int? Sata3Count { get; set; }
        public int? PowerMotherboard { get; set; }
        public int? FanQuantity { get; set; }

        [MaxLength(50)]
        public string? Ethernet { get; set; }

        [MaxLength(100)]
        public string? Audio { get; set; }

        [MaxLength(50)]
        public string? Wifi { get; set; }

        [MaxLength(50)]
        public string? Bluetooth { get; set; }

        [MaxLength(255)]
        public string? VideoPorts { get; set; }

        [MaxLength(50)]
        public string? FormFactor { get; set; }

        [MaxLength(50)]
        public string? SizeDimentions { get; set; }

        public int? Wattage { get; set; }

        [Url]
        public string? FactoryLink { get; set; }

        public List<CpuPowerConnector> CpuPowerConnectors { get; set; } = new();
        public List<PcleSlot> PcleSlots { get; set; } = new();
        public List<M2Slot> M2Slots { get; set; } = new();
        public List<RearPort> RearPorts { get; set; } = new();
        public List<InnerPort> InnerPorts { get; set; } = new();

    }
}
