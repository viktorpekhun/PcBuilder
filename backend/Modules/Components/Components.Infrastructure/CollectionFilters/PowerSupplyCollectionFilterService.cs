using Components.Application.CollectionFilters;
using Components.Domain.Entities;
using PcBuilder.SharedKernel.Filtering;

namespace Components.Infrastructure.CollectionFilters
{
    public class PowerSupplyCollectionFilterService : IPowerSupplyCollectionFilterService
    {
        private const string CpuConnectorPinsKey = "cpuConnectorPins";
        private const string GpuConnectorPinsKey = "gpuConnectorPins";

        public IQueryable<PowerSupply> Apply(IQueryable<PowerSupply> query, ResourceParameters parameters)
        {
            if (parameters.Filters.TryGetValue(CpuConnectorPinsKey, out var cpuPinValues))
            {
                var pins = ParsePins(cpuPinValues);
                if (pins.Count > 0)
                    query = query.Where(p => p.PowerSupplyPowerConnectors
                        .Any(c => c.Type == "CPU" && pins.Contains(c.Pins + (c.AdditionalPins ?? 0))));
            }

            if (parameters.Filters.TryGetValue(GpuConnectorPinsKey, out var gpuPinValues))
            {
                var pins = ParsePins(gpuPinValues);
                if (pins.Count > 0)
                    query = query.Where(p => p.PowerSupplyPowerConnectors
                        .Any(c => c.Type == "GPU" && pins.Contains(c.Pins + (c.AdditionalPins ?? 0))));
            }

            return query;
        }

        private static List<int> ParsePins(string[] values) =>
            values
                .Select(v => int.TryParse(v, out var n) ? (int?)n : null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .ToList();
    }
}
