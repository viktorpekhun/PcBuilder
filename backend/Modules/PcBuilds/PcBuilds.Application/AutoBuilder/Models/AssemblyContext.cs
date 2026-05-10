namespace PcBuilds.Application.AutoBuilder.Models
{
    public record AssemblyContext(
        AutoBuildRequestDto Request,
        ScenarioPolicy Policy,
        CandidatePool Pool);
}
