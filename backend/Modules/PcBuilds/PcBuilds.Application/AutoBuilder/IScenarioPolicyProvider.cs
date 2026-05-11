using PcBuilds.Application.AutoBuilder.Models;

namespace PcBuilds.Application.AutoBuilder
{
    public interface IScenarioPolicyProvider
    {
        ScenarioPolicy GetPolicy(UsageScenario scenario);
    }
}
