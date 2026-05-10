using PcBuilds.Application.AutoBuilder.Models;

namespace PcBuilds.Application.AutoBuilder
{
    public interface ICandidatePruner
    {
        Task<CandidatePool> PruneAsync(AutoBuildRequestDto request, ScenarioPolicy policy, CancellationToken ct);
    }
}
