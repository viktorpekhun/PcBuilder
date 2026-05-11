using Components.Domain.Entities;
using PcBuilds.Application.AutoBuilder;

namespace PcBuilds.Infrastructure.AutoBuilder
{
    public class CoolerSelector : ICoolerSelector
    {
        private static readonly HashSet<string> LegacySuffixes = new(StringComparer.OrdinalIgnoreCase)
            { "F", "K", "X", "KF", "KS", "XT", "XTX" };

        public CpuCooler? PickCooler(Cpu cpu, IReadOnlyList<CpuCooler> coolerCandidates)
        {
            if (cpu.Tdp <= 65 && !NeedsDedicatedCooler(cpu))
                return null;

            var qualified = coolerCandidates
                .Where(c => InferMaxPowerDissipation(c) >= (cpu.Tdp ?? 0))
                .Where(c => c.AveragePrice.HasValue && c.AveragePrice > 0)
                .OrderByDescending(c => InferMaxPowerDissipation(c) / (double)c.AveragePrice!)
                .ToList();

            return qualified.FirstOrDefault();
        }

        public int InferMaxPowerDissipation(CpuCooler cooler)
        {
            if (cooler.MaxPowerDissipation.HasValue && cooler.MaxPowerDissipation > 0)
                return cooler.MaxPowerDissipation.Value;

            if (string.Equals(cooler.Type, "Water", StringComparison.OrdinalIgnoreCase)
                || string.Equals(cooler.Type, "Liquid", StringComparison.OrdinalIgnoreCase))
            {
                return (int?)cooler.FanSize switch
                {
                    120 or 140 => 150,
                    240 or 280 => 250,
                    360 or 420 => 350,
                    _ => 0
                };
            }

            // Air cooler without MaxPowerDissipation — unreliable, exclude
            return 0;
        }

        private static bool NeedsDedicatedCooler(Cpu cpu)
        {
            if (cpu.Name.Contains("Threadripper", StringComparison.OrdinalIgnoreCase))
                return true;

            var nameParts = cpu.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length == 0) return false;
            var lastToken = nameParts[^1];
            foreach (var suffix in LegacySuffixes)
            {
                if (lastToken.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                    && lastToken.Length > suffix.Length
                    && char.IsDigit(lastToken[^(suffix.Length + 1)]))
                    return true;
            }
            return false;
        }
    }
}
