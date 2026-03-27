using Components.Domain.Entities;
using PcBuilds.Domain.Entities;
using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility.Rules
{
    public class PowerSupplyCpuConnectorRule : ICompatibilityRule
    {
        public string Name => "Power Supply and CPU Connector Compatibility";
        public CompatibilityResult Check(PcBuild pcBuild)
        {
            var result = new CompatibilityResult();
            var motherboard = pcBuild.Motherboard;
            var powerSupply = pcBuild.PowerSupply;

            if (motherboard == null || powerSupply == null)
                return result;

            var cpuPowerConnectors = motherboard.CpuPowerConnectors;
            var powerSupplyConnectors = powerSupply.PowerSupplyPowerConnectors;

            if (cpuPowerConnectors == null || !cpuPowerConnectors.Any() ||
                powerSupplyConnectors == null || !powerSupplyConnectors.Any())
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = "Неможливо перевірити сумісність роз'єму живлення CPU на Материнській платі та Блоку живлення — недостатньо даних."
                });
                return result;
            }

            var psuCpuConnectors = powerSupplyConnectors
                .Where(c => c.Type == "CPU")
                .ToList();

            if (!psuCpuConnectors.Any())
            {
                result.Messages.Add(new CompatibilityMessage
                {
                    Type = CompatibilityMessageType.Warning,
                    Message = "Неможливо перевірити сумісність роз'єму живлення CPU на Материнській платі та Блоку живлення — недостатньо даних."
                });
                return result;
            }

            var availableConnectors = psuCpuConnectors
                .Select(c => new {
                    Pins = c.Pins,
                    AdditionalPins = c.AdditionalPins,
                    RemainingQuantity = c.Quantity
                })
                .ToList();

            var requiredConnectors = cpuPowerConnectors
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
                    else if (connector.Pins == pinCount * 2 && (connector.AdditionalPins ?? 0) == 0)
                    {
                        int used = Math.Min(connector.RemainingQuantity * 2, neededQuantity - satisfiedQuantity);
                        satisfiedQuantity += used;
                        availableConnectors[i] = new
                        {
                            connector.Pins,
                            connector.AdditionalPins,
                            RemainingQuantity = connector.RemainingQuantity - (used / 2 + used % 2) 
                        };
                    }
                    else if (connector.Pins == pinCount && connector.AdditionalPins != null && connector.AdditionalPins > 0)
                    {
                        int used = Math.Min(connector.RemainingQuantity * 2, neededQuantity - satisfiedQuantity);
                        satisfiedQuantity += used;
                        availableConnectors[i] = new
                        {
                            connector.Pins,
                            connector.AdditionalPins,
                            RemainingQuantity = connector.RemainingQuantity - (used / 2 + used % 2)
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
                    Message = $"Блок живлення не має достатньо CPU-конекторів для материнської плати: {string.Join(", ", incompatibleConnectors)}"
                });
            }

            return result;
        }
    }
}