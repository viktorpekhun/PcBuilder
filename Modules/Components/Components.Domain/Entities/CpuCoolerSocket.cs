using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Components.Domain.Entities
{
    public class CpuCoolerSocket
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SocketType { get; set; } = string.Empty;

        [Required]
        public Guid CpuCoolerId { get; set; }

        [ForeignKey("CpuCoolerId")]
        public CpuCooler CpuCooler { get; set; } = null!;
    }
}
