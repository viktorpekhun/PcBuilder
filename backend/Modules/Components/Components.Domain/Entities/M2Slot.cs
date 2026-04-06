namespace Components.Domain.Entities
{
    public class M2Slot
    {
        public Guid Id { get; set; }
        public double Version { get; set; }
        public int? Lane { get; set; }
        public int? Quantity { get; set; }

        public Guid MotherboardId { get; set; }

        public Motherboard Motherboard { get; set; } = null!;
    }
}
