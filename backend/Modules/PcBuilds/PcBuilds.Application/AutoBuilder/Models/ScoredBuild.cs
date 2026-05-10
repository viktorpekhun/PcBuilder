using PcBuilds.Application.Compatibility;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.AutoBuilder.Models
{
    public record ScoredBuild(PcBuild Build, BuildCompatibilityReport Report, decimal Price);
}
