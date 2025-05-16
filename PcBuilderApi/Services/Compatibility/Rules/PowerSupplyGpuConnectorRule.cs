using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility.Rules
{
    public class PowerSupplyGpuConnectorRule : ICompatibilityRule
    {
        public string Name => "Power Supply and GPU Connector Compatibility";

        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var gpu = pcBuild.Gpu;
            var powerSupply = pcBuild.PowerSupply;

            if (gpu == null || powerSupply == null)
                return result;

            var gpuPowerConnectors = gpu.GpuPowerConnectors;
            var powerSupplyConnectors = powerSupply.PowerSupplyPowerConnectors;

            if (gpuPowerConnectors == null || !gpuPowerConnectors.Any() ||
                powerSupplyConnectors == null || !powerSupplyConnectors.Any())
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = "Неможливо перевірити сумісність Відеокарти та Блоку живлення — недостатньо даних."
                });
                return result;
            }

            var psuGpuConnectors = powerSupplyConnectors
                .Where(c => c.Type == "GPU")
                .ToList();

            if (!psuGpuConnectors.Any())
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = "Неможливо перевірити сумісність Відеокарти та Блоку живлення — недостатньо даних."
                });
                return result;
            }

            var availableConnectors = psuGpuConnectors
                .Select(c => new {
                    Pins = c.Pins,
                    AdditionalPins = c.AdditionalPins,
                    RemainingQuantity = c.Quantity
                })
                .ToList();

            var requiredConnectors = gpuPowerConnectors
                .GroupBy(c => c.Pins)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity));

            var incompatibleConnectors = new List<string>();

            foreach (var requirement in requiredConnectors)
            {
                int pinCount = requirement.Key;
                int neededQuantity = requirement.Value;
                int satisfiedQuantity = 0;

                for (int i = 0; i < availableConnectors.Count; i++)
                {
                    var connector = availableConnectors[i];

                    if (connector.Pins == pinCount)
                    {
                        int used = Math.Min(connector.RemainingQuantity, neededQuantity - satisfiedQuantity);
                        satisfiedQuantity += used;
                        availableConnectors[i] = new
                        {
                            connector.Pins,
                            connector.AdditionalPins,
                            RemainingQuantity = connector.RemainingQuantity - used
                        };
                    }

                    else if (connector.Pins + (connector.AdditionalPins ?? 0) == pinCount)
                    {
                        int used = Math.Min(connector.RemainingQuantity, neededQuantity - satisfiedQuantity);
                        satisfiedQuantity += used;
                        availableConnectors[i] = new
                        {
                            connector.Pins,
                            connector.AdditionalPins,
                            RemainingQuantity = connector.RemainingQuantity - used
                        };
                    }

                    else if (connector.Pins + (connector.AdditionalPins ?? 0) >= pinCount &&
                             connector.Pins <= pinCount)
                    {
                        int used = Math.Min(connector.RemainingQuantity, neededQuantity - satisfiedQuantity);
                        satisfiedQuantity += used;
                        availableConnectors[i] = new
                        {
                            connector.Pins,
                            connector.AdditionalPins,
                            RemainingQuantity = connector.RemainingQuantity - used
                        };
                    }

                    if (satisfiedQuantity >= neededQuantity)
                        break;
                }

                if (satisfiedQuantity < neededQuantity)
                {
                    incompatibleConnectors.Add($"{pinCount}-pin (потрібно {neededQuantity}, доступно {satisfiedQuantity})");
                }
            }

            if (incompatibleConnectors.Any())
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Problem,
                    Message = $"Блок живлення не має достатньо конекторів для відеокарти: {string.Join(", ", incompatibleConnectors)}"
                });
            }

            return result;
        }

    }
}
