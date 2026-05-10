namespace PcBuilds.Application.AutoBuilder
{
    /// <summary>
    /// IDs of the specific components the autobuilder picked for each slot.
    /// The build itself is not persisted; this lets the caller resolve full component data on demand.
    /// </summary>
    public record SelectedComponentsDto(
        Guid? CpuId,
        Guid? GpuId,
        Guid? MotherboardId,
        Guid? CpuCoolerId,
        Guid? PowerSupplyId,
        Guid? PcCaseId,
        Guid? RamId,
        int RamQuantity,
        Guid? SsdId,
        int SsdQuantity,
        Guid? HddId,
        int HddQuantity,
        Guid? FanId,
        int FanQuantity);
}
