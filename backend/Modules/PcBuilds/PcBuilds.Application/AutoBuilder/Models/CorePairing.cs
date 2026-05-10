using Components.Domain.Entities;

namespace PcBuilds.Application.AutoBuilder.Models
{
    public record CorePairing(
        Cpu Cpu,
        Gpu Gpu,
        decimal CombinedPrice,
        double CombinedValueScore);
}
