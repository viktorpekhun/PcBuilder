using PcBuilder.SharedKernel;

namespace PcBuilds.Application.AutoBuilder
{
    public interface IAutoBuilderService
    {
        Task<Result<AutoBuildResultDto>> BuildAsync(AutoBuildRequestDto request, CancellationToken ct);
    }
}
