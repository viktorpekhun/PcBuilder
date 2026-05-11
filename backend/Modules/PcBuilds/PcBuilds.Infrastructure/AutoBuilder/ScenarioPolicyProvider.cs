using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.AutoBuilder.Models;

namespace PcBuilds.Infrastructure.AutoBuilder
{
    public class ScenarioPolicyProvider : IScenarioPolicyProvider
    {
        private static readonly Dictionary<UsageScenario, ScenarioPolicy> Policies = new()
        {
            [UsageScenario.Gaming] = new ScenarioPolicy(
                new BudgetAllocation(Cpu: 0.18, Gpu: 0.42, Motherboard: 0.10, Cooler: 0.04, Psu: 0.09, Case: 0.05, Ram: 0.06, Ssd: 0.06, Hdd: 0.00, Fans: 0.00),
                new ComfortThresholds(MinRamGb: 32, MinSsdGb: 1000, MinHddGb: null, MinPsuW: 600),
                new BalanceBounds(Min: 0.8, Max: 1.6)),

            [UsageScenario.Office] = new ScenarioPolicy(
                new BudgetAllocation(Cpu: 0.35, Gpu: 0.15, Motherboard: 0.15, Cooler: 0.03, Psu: 0.09, Case: 0.05, Ram: 0.06, Ssd: 0.07, Hdd: 0.00, Fans: 0.00),
                new ComfortThresholds(MinRamGb: 16, MinSsdGb: 500, MinHddGb: null, MinPsuW: 300),
                new BalanceBounds(Min: 0.0, Max: 0.6)),

            [UsageScenario.ContentCreation] = new ScenarioPolicy(
                new BudgetAllocation(Cpu: 0.25, Gpu: 0.30, Motherboard: 0.10, Cooler: 0.05, Psu: 0.08, Case: 0.05, Ram: 0.09, Ssd: 0.06, Hdd: 0.02, Fans: 0.00),
                new ComfortThresholds(MinRamGb: 32, MinSsdGb: 2000, MinHddGb: 2000, MinPsuW: 650),
                new BalanceBounds(Min: 0.6, Max: 1.3)),

            [UsageScenario.Workstation] = new ScenarioPolicy(
                new BudgetAllocation(Cpu: 0.35, Gpu: 0.20, Motherboard: 0.10, Cooler: 0.06, Psu: 0.09, Case: 0.05, Ram: 0.10, Ssd: 0.05, Hdd: 0.00, Fans: 0.00),
                new ComfortThresholds(MinRamGb: 64, MinSsdGb: 2000, MinHddGb: 4000, MinPsuW: 850),
                new BalanceBounds(Min: 0.3, Max: 1.0)),

            [UsageScenario.Budget] = new ScenarioPolicy(
                new BudgetAllocation(Cpu: 0.25, Gpu: 0.30, Motherboard: 0.15, Cooler: 0.02, Psu: 0.10, Case: 0.07, Ram: 0.06, Ssd: 0.05, Hdd: 0.00, Fans: 0.00),
                new ComfortThresholds(MinRamGb: 16, MinSsdGb: 500, MinHddGb: null, MinPsuW: 400),
                new BalanceBounds(Min: 0.6, Max: 1.5)),
        };

        public ScenarioPolicy GetPolicy(UsageScenario scenario) => Policies[scenario];
    }
}
