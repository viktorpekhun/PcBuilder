using System.ComponentModel.DataAnnotations;
using Auth.Domain.Entities;
using Components.Domain.Entities;

namespace PcBuilds.Domain.Entities
{
    public class PcBuild
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255, ErrorMessage = "Name can't have more than 255 characters.")]
        [RegularExpression(@"^[a-zA-Zа-яА-ЯіІїЇєЄґҐёЁ0-9 _-]*$", ErrorMessage = "Name can only contain letters, numbers, underscores (_) and hyphens (-)")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description can't have more than 500 characters.")]
        public string? Description { get; set; } = string.Empty;

        public decimal Price { get; set; } = 0;

        [Range(0, 5, ErrorMessage = "Average rating must be between 0.00 and 5.00.")]
        public decimal AverageRating { get; set; } = 0;

        public bool IsPublished { get; set; } = false;
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;


        public Guid? CpuId { get; set; }
        public Cpu? Cpu { get; set; }
        public Guid? CpuOfferId { get; set; }
        public ProductOffer? CpuOffer { get; set; }


        public Guid? GpuId { get; set; }
        public Gpu? Gpu { get; set; }
        public Guid? GpuOfferId { get; set; }
        public ProductOffer? GpuOffer { get; set; }


        public Guid? MotherboardId { get; set; }
        public Motherboard? Motherboard { get; set; }
        public Guid? MotherboardOfferId { get; set; }
        public ProductOffer? MotherboardOffer { get; set; }


        public Guid? CpuCoolerId { get; set; }
        public CpuCooler? CpuCooler { get; set; }
        public Guid? CpuCoolerOfferId { get; set; }
        public ProductOffer? CpuCoolerOffer { get; set; }


        public Guid? PowerSupplyId { get; set; }
        public PowerSupply? PowerSupply { get; set; }
        public Guid? PowerSupplyOfferId { get; set; }
        public ProductOffer? PowerSupplyOffer { get; set; }


        public Guid? PcCaseId { get; set; }
        public PcCase? PcCase { get; set; }
        public Guid? PcCaseOfferId { get; set; }
        public ProductOffer? PcCaseOffer { get; set; }


        public List<PcBuild_Ssd> PcBuild_Ssds { get; set; } = new();
        public List<PcBuild_Hdd> PcBuild_Hdds { get; set; } = new();
        public List<PcBuild_Ram> PcBuild_Rams { get; set; } = new();
        public List<PcBuild_Fan> PcBuild_Fans { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
