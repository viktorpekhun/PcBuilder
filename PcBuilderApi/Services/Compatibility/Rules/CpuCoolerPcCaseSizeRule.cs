using Microsoft.IdentityModel.Tokens;
using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility.Rules
{
    public class CpuCoolerPcCaseSizeRule : ICompatibilityRule
    {
        public string Name => "CPU Cooler and PC Case Size Compatibility";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var cpuCooler = pcBuild.CpuCooler;
            var pcCase = pcBuild.PcCase;
            if (cpuCooler == null || pcCase == null)
            {
                return result;
            }

            if (cpuCooler.Type == "Air")
            {
                if (cpuCooler.Height == null || pcCase.MaxCpuCoolerHeight == null)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = "Неможливо перевірити сумісність кулера CPU та корпусу ПК — недостатньо даних."
                    });
                    return result;
                }
                if (cpuCooler.Height > pcCase.MaxCpuCoolerHeight)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Problem,
                        Message = $"Висота кулера CPU ({cpuCooler.Height} мм) перевищує максимальну висоту кулера в корпусі ПК ({pcCase.MaxCpuCoolerHeight} мм)."
                    });
                }
            }
            else
            {
                var pcCaseFanLocations = pcCase.PcCaseFanLocations;
                if (cpuCooler.FanCount == null || cpuCooler.FanSize == null || pcCaseFanLocations == null || !pcCaseFanLocations.Any())
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Warning,
                        Message = "Неможливо перевірити сумісність рідинного охолодження CPU та корпусу ПК — недостатньо даних."
                    });
                    return result;
                }
                var matches = pcCaseFanLocations.Where(x => x.FanSize == cpuCooler.FanSize && x.MaxFans >= cpuCooler.FanCount).ToList();
                if (matches.Count == 0)
                {
                    result.Messages.Add(new CompatibilityMessage
                    {
                        Type = CompatibilityMessageType.Problem,
                        Message = $"Рідинне охолодження CPU не сумісне з корпусом ПК ({pcCase.Name}) - недостатньо місця в корпусі."
                    });
                }
            }
            return result;
        }
    }
}
