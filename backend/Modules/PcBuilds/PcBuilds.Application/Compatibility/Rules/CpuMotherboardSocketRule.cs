using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class CpuMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "CpuMotherboardSocket";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var cpu = pcBuild.Cpu;
            var motherboard = pcBuild.Motherboard;

            if (cpu == null || motherboard == null)
                return result;

            if (string.IsNullOrEmpty(cpu.Socket) || string.IsNullOrEmpty(motherboard.Socket))
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.SocketDataMissing,
                    Parameters = new() { ["CpuSocket"] = cpu.Socket ?? "", ["MbSocket"] = motherboard.Socket ?? "" }
                });
                return result;
            }

            if (cpu.Socket != motherboard.Socket)
            {
                result.FitnessScore = 0.0;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Critical,
                    Code = IssueCodes.SocketMismatch,
                    Parameters = new() { ["CpuSocket"] = cpu.Socket, ["MbSocket"] = motherboard.Socket }
                });
            }

            return result;
        }
    }
}
