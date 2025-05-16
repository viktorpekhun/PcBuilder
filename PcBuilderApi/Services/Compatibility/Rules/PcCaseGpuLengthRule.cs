using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility.Rules
{
    public class PcCaseGpuLengthRule : ICompatibilityRule
    {
        public string Name => "GPU and PC Case Length Compatibility Rule";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var gpu = pcBuild.Gpu;
            var pcCase = pcBuild.PcCase;
            if (gpu == null || pcCase == null)
            {
                return result;
            }
            if (gpu.SizeLength == null || pcCase.MaxGpuLength == null)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = $"Неможливо перевірити сумісність Відеокарти та Корпусу ПК — недостатньо даних."
                });
                return result;
            }
            if (gpu.SizeLength > pcCase.MaxGpuLength)
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Problem,
                    Message = $"Довжина Відеокарти ({gpu.SizeLength} мм) перевищує максимальну довжину відеокарти в Корпусі ({pcCase.MaxGpuLength} мм)."
                });
            }
            return result;
        }
    }
}
