namespace Components.Domain.Entities
{
    public class CpuPowerConnector
    {
        public Guid Id { get; set; }

        public int Pins { get; set; }

        public int Quantity { get; set; }

        public Guid MotherboardId { get; set; }

        public Motherboard Motherboard { get; set; } = null!;
    }
}
