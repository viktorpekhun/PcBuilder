namespace PcBuilds.Application.AutoBuilder
{
    public record SelectedComponentInfo(Guid Id, string Name, decimal? AveragePrice);

    public record SelectedComponentsDto(
        SelectedComponentInfo? Cpu,
        SelectedComponentInfo? Gpu,
        SelectedComponentInfo? Motherboard,
        SelectedComponentInfo? CpuCooler,
        SelectedComponentInfo? PowerSupply,
        SelectedComponentInfo? PcCase,
        SelectedComponentInfo? Ram,
        int RamQuantity,
        SelectedComponentInfo? Ssd,
        int SsdQuantity,
        SelectedComponentInfo? Hdd,
        int HddQuantity,
        SelectedComponentInfo? Fan,
        int FanQuantity);
}
