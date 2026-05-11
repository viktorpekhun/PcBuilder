namespace PcBuilds.Application.AutoBuilder.Models
{
    public record ComfortThresholds(
        int MinRamGb,
        int MinSsdGb,
        int? MinHddGb,
        int MinPsuW);
}
