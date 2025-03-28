using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PcBuilderApi.Models
{
    public class PowerSupplyPowerConnector
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty;

        [Required]
        public int Pins { get; set; }

        public int? AdditionalPins { get; set; }

        [Required]
        public int Quantity { get; set; }


        [Required]
        public Guid PowerSupplyId { get; set; }

        [ForeignKey("PowerSupplyId")]
        public PowerSupply PowerSupply { get; set; } = null!;

    }
}
