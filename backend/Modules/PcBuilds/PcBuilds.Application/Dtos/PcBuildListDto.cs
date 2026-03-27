namespace PcBuilds.Application.Dtos
{
    public class PcBuildListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
