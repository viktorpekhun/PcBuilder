using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class CpuCoolerMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "CoolerSocket";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var cpuCooler = pcBuild.CpuCooler;
            var motherboard = pcBuild.Motherboard;

            if (cpuCooler == null || motherboard == null)
                return result;

            var cpuCoolerSockets = cpuCooler.CpuCoolerSockets;

            if (string.IsNullOrEmpty(motherboard.Socket) || cpuCoolerSockets == null || !cpuCoolerSockets.Any())
            {
                result.FitnessScore = 0.7;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Warning,
                    Code = IssueCodes.CoolerSocketDataMissing,
                    Parameters = new() { ["CoolerName"] = cpuCooler.Name ?? "", ["MbSocket"] = motherboard.Socket ?? "" }
                });
                return result;
            }

            bool isCompatible = false;

            foreach (var socket in cpuCoolerSockets)
            {
                if (motherboard.Socket.Contains("Socket 115") && socket.SocketType.Contains("Socket 115"))
                    return result;

                if (motherboard.Socket == socket.SocketType)
                {
                    isCompatible = true;
                    break;
                }
            }

            if (!isCompatible)
            {
                result.FitnessScore = 0.0;
                result.Issues.Add(new CompatibilityIssue
                {
                    Severity = CompatibilitySeverity.Critical,
                    Code = IssueCodes.CoolerSocketMismatch,
                    Parameters = new() { ["CoolerName"] = cpuCooler.Name ?? "", ["MbSocket"] = motherboard.Socket }
                });
            }

            return result;
        }
    }
}
