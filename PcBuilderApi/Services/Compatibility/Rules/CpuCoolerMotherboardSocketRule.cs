using Microsoft.IdentityModel.Tokens;
using PcBuilderApi.Models;
using System.Net.Sockets;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility.Rules
{
    public class CpuCoolerMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "CPU Cooler and Motherboard Socket Compatibility";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();

            var cpuCooler = pcBuild.CpuCooler;
            var motherboard = pcBuild.Motherboard;

            if (cpuCooler == null || motherboard == null)
            {
                return result;
            }

            var cpuCoolerSockets = cpuCooler.CpuCoolerSockets;

            if (string.IsNullOrEmpty(motherboard.Socket) || cpuCoolerSockets == null || !cpuCoolerSockets.Any())
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = "Неможливо перевірити сумісність кулера CPU та материнської плати — недостатньо даних."
                });
                return result;
            }

            bool isCompatible = false;

            foreach (var socket in cpuCoolerSockets)
            {
                if (motherboard.Socket.Contains("Socket 115") && socket.SocketType.Contains("Socket 115"))
                {
                    return result;
                }
                else if (motherboard.Socket == socket.SocketType)
                {
                    isCompatible = true;
                    break;
                }
            }
            if (!isCompatible)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Problem,
                    Message = $"Кулер CPU не сумісний з сокетом материнської плати ({motherboard.Socket})."
                });
            }

            return result;
        }
    }
}
