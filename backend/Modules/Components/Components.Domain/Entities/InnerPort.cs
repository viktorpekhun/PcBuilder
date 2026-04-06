namespace Components.Domain.Entities
{
    public class InnerPort
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public Guid MotherboardId { get; set; }

        public Motherboard Motherboard { get; set; } = null!;
    }
}
