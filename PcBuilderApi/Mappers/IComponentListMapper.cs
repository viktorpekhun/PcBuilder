using PcBuilderApi.Models;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Mappers
{
    public interface IComponentListMapper
    {
        ComponentType ComponentType { get; }
        IEnumerable<object> MapAll(IEnumerable<object> entities);
    }
}
