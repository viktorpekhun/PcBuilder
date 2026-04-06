namespace Components.Domain.Entities
{
    public class PowerSupplyPowerConnector
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public int Pins { get; set; }

        public int? AdditionalPins { get; set; }

        public int Quantity { get; set; }

        public Guid PowerSupplyId { get; set; }

        public PowerSupply PowerSupply { get; set; } = null!;
    }
}
