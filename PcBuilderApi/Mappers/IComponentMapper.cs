using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers
{
    public interface IComponentMapper
    {
        ComponentType ComponentType { get; }
        object MapById(object entity, IEnumerable<ProductOffer> productOffers);
    }
}
