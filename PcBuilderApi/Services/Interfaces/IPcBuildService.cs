using PcBuilderApi.Models;
using PcBuilderApi.Services.Compatibility;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Interfaces
{
    public interface IPcBuildService
    {
        Task<bool> SaveBuildAsync(PcBuild pcBuild);
        Task<bool> DeleteBuildAsync(Guid pcBuildId);
        Task<PcBuild?> GetBuildByIdAsync(Guid pcBuildId);
        Task<List<PcBuild>> GetAllBuildsAsync();
        Task<List<PcBuild>> GetUserBuildsAsync(Guid userId);
        Task<bool> UpdateBuildAsync(PcBuild pcBuild);
        Task<List<CompatibilityResult>> CheckComponentCompatibilityAsync(PcBuild pcBuild, Guid componentId, ComponentType componentType);
        Task<bool> RemoveComponentFromBuildAsync(PcBuild pcBuild, Guid componentId, ComponentType componentType);
    }
}
