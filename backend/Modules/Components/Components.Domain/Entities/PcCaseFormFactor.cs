namespace Components.Domain.Entities
{
    public class PcCaseFormFactor
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid PcCaseId { get; set; }

        public PcCase PcCase { get; set; } = null!;
    }
}
