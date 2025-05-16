using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;
using System.Reflection.Metadata;

namespace PcBuilderApi.Services.Compatibility.Rules
{
    public class CpuMotherboardSocketRule : ICompatibilityRule
    {
        public string Name => "CPU and Motherboard Socket Compatibility";
        public CompatibilityResult Check(PcBuild pcBuild)
        {

            var result = new CompatibilityResult();

            var cpu = pcBuild.Cpu;
            var motherboard = pcBuild.Motherboard;

            if (cpu == null || motherboard == null)
            {
                return result;
            }

            if (string.IsNullOrEmpty(pcBuild.Cpu.Socket) || string.IsNullOrEmpty(pcBuild.Motherboard.Socket))
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = "Неможливо перевірити сумісність CPU та материнської плати — недостатньо даних."
                });
                return result;
            }

            if (cpu.Socket != motherboard.Socket)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Problem,
                    Message = $"Сокет CPU ({cpu.Socket}) не збігається з сокетом материнської плати ({motherboard.Socket})."
                });
            }

            return result;
        }

    }
}
