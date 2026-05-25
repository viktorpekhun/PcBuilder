using Components.Domain.Entities;
using PcBuilder.SharedKernel.Filtering;

namespace Components.Application.CollectionFilters
{
    public interface IPowerSupplyCollectionFilterService
    {
        /// <summary>
        /// Extracts collection-based filter keys from <paramref name="parameters"/> (consuming them so the
        /// generic engine never sees them), applies the corresponding .Any() predicates to the query, and
        /// returns the narrowed queryable.
        /// </summary>
        IQueryable<PowerSupply> Apply(IQueryable<PowerSupply> query, ResourceParameters parameters);
    }
}
