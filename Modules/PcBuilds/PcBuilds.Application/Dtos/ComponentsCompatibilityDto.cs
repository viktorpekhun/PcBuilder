namespace PcBuilds.Application.Dtos
{
    public class ComponentsCompatibilityDto
    {
        public Guid? CpuId { get; set; }
        public Guid? MotherboardId { get; set; }
        public Guid? GpuId { get; set; }
        public Guid? PowerSupplyId { get; set; }
        public Guid? CpuCoolerId { get; set; }
        public Guid? PcCaseId { get; set; }

        public List<ComponentQuantityDto> Rams { get; set; } = new();
        public List<ComponentQuantityDto> Ssds { get; set; } = new();
        public List<ComponentQuantityDto> Hdds { get; set; } = new();
        public List<ComponentQuantityDto> Fans { get; set; } = new();
    }
}
