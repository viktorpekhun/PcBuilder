namespace PcBuilds.Application.Dtos
{
    public class PcBuildGalleryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int ComponentCount { get; set; }
        public int CommentCount { get; set; }
    }
}
