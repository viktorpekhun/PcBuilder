using PcBuilderApi.Utilities.Filtering;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Interfaces
{
    public interface IComponentService
    {
        Task<PagedResponse<object>> GetAllByTypeAsync(ComponentType componentType, ResourceParameters parameters);

        Task<object> GetByIdAsync(Guid id, ComponentType componentType);
    }
}
