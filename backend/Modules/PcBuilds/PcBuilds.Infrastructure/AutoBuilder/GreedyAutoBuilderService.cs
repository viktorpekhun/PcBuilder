using PcBuilder.SharedKernel;
using PcBuilds.Application.AutoBuilder;

namespace PcBuilds.Infrastructure.AutoBuilder
{
    // TODO: implement greedy algorithm once Phases 1-4 are stable.
    // Algorithm sketch: allocate budget by scenario weights → query candidates per slot →
    // score by (PassMarkScore / price) ratio → run CompatibilityChecker as fitness oracle →
    // return best strictly-compatible build by OverallFitnessScore.
    public class GreedyAutoBuilderService : IAutoBuilderService
    {
        public Task<Result<AutoBuildResultDto>> BuildAsync(AutoBuildRequestDto request, CancellationToken ct)
            => Task.FromResult(Result.Failure<AutoBuildResultDto>(new Error("NOT_IMPLEMENTED", "AutoBuilder is not yet implemented.", 501)));
    }
}
