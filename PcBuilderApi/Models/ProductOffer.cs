using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Models
{
    public class ProductOffer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public ComponentType ComponentType { get; set; }

        [Required]
        public Guid ComponentId { get; set; }

        [Required]
        public string ProductOfferUrl { get; set; } = string.Empty;

        [Required]
        public Guid StoreId { get; set; }

        [ForeignKey("StoreId")]
        public Store Store { get; set; } = null!;
    }
}
