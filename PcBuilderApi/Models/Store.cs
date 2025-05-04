using System.ComponentModel.DataAnnotations;

namespace PcBuilderApi.Models
{
    public class Store
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }

        [Required]
        public int Likes { get; set; }

        [Required]
        public int Dislikes { get; set; }

        public List<ProductOffer> ProductOffers { get; set; } = new();
    }
}
