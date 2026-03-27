namespace PcBuilds.Application.Dtos
{
    public class PcBuildInputDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

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
