namespace PcBuilderApi.Dtos.PcBuildDtos
{
    public class PcBuildRequestDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public decimal Price { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ComponentPreviewDto? Cpu { get; set; }

        public ComponentPreviewDto? Gpu { get; set; }
        public ComponentPreviewDto? Motherboard { get; set; }
        public ComponentPreviewDto? CpuCooler { get; set; }
        public ComponentPreviewDto? PowerSupply { get; set; }
        public ComponentPreviewDto? PcCase { get; set; }

        public List<MultiComponentPreviewDto> Rams { get; set; } = new();
        public List<MultiComponentPreviewDto> Ssds { get; set; } = new();
        public List<MultiComponentPreviewDto> Hdds { get; set; } = new();
        public List<MultiComponentPreviewDto> Fans { get; set; } = new();
    }
    public class ComponentPreviewDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? StoreName { get; set; }
        public decimal? Price { get; set; }

    }
    public class MultiComponentPreviewDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }

        public string? StoreName { get; set; }
        public decimal? TotalPrice { get; set; }


    }
}
