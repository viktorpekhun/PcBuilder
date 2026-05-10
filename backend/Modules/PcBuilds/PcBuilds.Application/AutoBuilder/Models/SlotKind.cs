namespace PcBuilds.Application.AutoBuilder.Models
{
    /// <summary>
    /// Component slots filled in order by the sequential assembler. The slot queue is
    /// built per-assembly so optional slots (HDD, Fan) can be conditionally included or
    /// dropped based on the scenario policy and chosen components.
    /// </summary>
    public enum SlotKind
    {
        Motherboard,
        Ram,
        Cooler,
        Ssd,
        Hdd,
        Case,
        Fan,
        Psu
    }
}
