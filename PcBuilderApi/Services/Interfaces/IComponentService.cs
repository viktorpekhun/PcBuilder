using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Interfaces
{
    public interface IComponentService
    {
        Task<IEnumerable<object>> GetAllByTypeAsync(ComponentType componentType);

        Task<object> GetByIdAsync(Guid id, ComponentType componentType);
    }
}
