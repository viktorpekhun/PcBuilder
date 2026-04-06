namespace Components.Domain.Entities
{
    public class GpuPowerConnector
    {
        public Guid Id { get; set; }

        public int Pins { get; set; }

        public int Quantity { get; set; }

        public Guid GpuId { get; set; }

        public Gpu Gpu { get; set; } = null!;
    }
}
