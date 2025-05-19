using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class PcBuild
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255, ErrorMessage = "Name can't have more than 255 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "Name can only contain letters, numbers, underscores (_) and hyphens (-)")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(500, ErrorMessage = "Description can't have more than 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; } = 0;

        [Range(0, 5, ErrorMessage = "Average rating must be between 0.00 and 5.00.")]
        public decimal AverageRating { get; set; } = 0;

        public bool IsPublished { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;


        public Guid? CpuId { get; set; }
        [ForeignKey("CpuId")]
        public Cpu? Cpu { get; set; }
        public Guid? CpuOfferId { get; set; }
        [ForeignKey("CpuOfferId")]
        public ProductOffer? CpuOffer { get; set; }


        public Guid? GpuId { get; set; }
        [ForeignKey("GpuId")]
        public Gpu? Gpu { get; set; }
        public Guid? GpuOfferId { get; set; }
        [ForeignKey("GpuOfferId")]
        public ProductOffer? GpuOffer { get; set; }


        public Guid? MotherboardId { get; set; }
        [ForeignKey("MotherboardId")]
        public Motherboard? Motherboard { get; set; }
        public Guid? MotherboardOfferId { get; set; }
        [ForeignKey("MotherboardOfferId")]
        public ProductOffer? MotherboardOffer { get; set; }


        public Guid? CpuCoolerId { get; set; }
        [ForeignKey("CpuCoolerId")]
        public CpuCooler? CpuCooler { get; set; }
        public Guid? CpuCoolerOfferId { get; set; }
        [ForeignKey("CpuCoolerOfferId")]
        public ProductOffer? CpuCoolerOffer { get; set; }


        public Guid? PowerSupplyId { get; set; }
        [ForeignKey("PowerSupplyId")]
        public PowerSupply? PowerSupply { get; set; }
        public Guid? PowerSupplyOfferId { get; set; }
        [ForeignKey("PowerSupplyOfferId")]
        public ProductOffer? PowerSupplyOffer { get; set; }


        public Guid? PcCaseId { get; set; }
        [ForeignKey("PcCaseId")]
        public PcCase? PcCase { get; set; }
        public Guid? PcCaseOfferId { get; set; }
        [ForeignKey("PcCaseOfferId")]
        public ProductOffer? PcCaseOffer { get; set; }


        public List<PcBuild_Ssd> PcBuild_Ssds { get; set; } = new();
        public List<PcBuild_Hdd> PcBuild_Hdds { get; set; } = new();
        public List<PcBuild_Ram> PcBuild_Rams { get; set; } = new();
        public List<PcBuild_Fan> PcBuild_Fans { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}
