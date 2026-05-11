namespace PcBuilds.Application.AutoBuilder.Models
{
    public record BudgetAllocation(
        double Cpu,
        double Gpu,
        double Motherboard,
        double Cooler,
        double Psu,
        double Case,
        double Ram,
        double Ssd,
        double Hdd,
        double Fans);
}
