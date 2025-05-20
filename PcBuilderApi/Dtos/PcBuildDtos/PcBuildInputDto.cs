using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Dtos.PcBuildDtos
{
    public class PcBuildInputDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255, ErrorMessage = "Name can't have more than 255 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9 _-]*$", ErrorMessage = "Name can contain letters, numbers, spaces, underscores (_) and hyphens (-)")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(500, ErrorMessage = "Description can't have more than 500 characters.")]
        public string Description { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = false;

        public Guid? CpuId { get; set; }
        public Guid? GpuId { get; set; }
        public Guid? MotherboardId { get; set; }
        public Guid? CpuCoolerId { get; set; }
        public Guid? PowerSupplyId { get; set; }
        public Guid? PcCaseId { get; set; }

        public Guid? CpuOfferId { get; set; }
        public Guid? GpuOfferId { get; set; }
        public Guid? MotherboardOfferId { get; set; }
        public Guid? CpuCoolerOfferId { get; set; }
        public Guid? PowerSupplyOfferId { get; set; }
        public Guid? PcCaseOfferId { get; set; }

        public List<ComponentQuantityDto> Rams { get; set; } = new();
        public List<ComponentQuantityDto> Ssds { get; set; } = new();
        public List<ComponentQuantityDto> Hdds { get; set; } = new();
        public List<ComponentQuantityDto> Fans { get; set; } = new();
    }

    public class ComponentQuantityDto
    {
        public Guid ComponentId { get; set; }
        public Guid? OfferId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
