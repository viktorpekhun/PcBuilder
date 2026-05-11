namespace Components.Application.PreFilter
{
    /// <summary>
    /// Component IDs for the already-chosen slots in a partial build.
    /// Any field may be null if that slot has not yet been filled.
    /// </summary>
    public record PartialBuildIds(
        Guid? CpuId = null,
        Guid? GpuId = null,
        Guid? MotherboardId = null,
        Guid? RamId = null,
        Guid? CpuCoolerId = null,
        Guid? PcCaseId = null,
        Guid? PowerSupplyId = null);
}
