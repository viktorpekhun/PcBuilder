namespace Components.Application.Dtos
{
    public class StoreDto
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}
