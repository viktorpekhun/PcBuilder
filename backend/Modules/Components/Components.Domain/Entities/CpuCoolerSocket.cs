namespace Components.Domain.Entities
{
    public class CpuCoolerSocket
    {
        public Guid Id { get; set; }

        public string SocketType { get; set; } = string.Empty;

        public Guid CpuCoolerId { get; set; }

        public CpuCooler CpuCooler { get; set; } = null!;
    }
}
