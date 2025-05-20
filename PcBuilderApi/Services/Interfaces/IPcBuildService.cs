using PcBuilderApi.Dtos.PcBuildDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Services.Compatibility;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Interfaces
{
    public interface IPcBuildService
    {
        Task<bool> SaveBuildAsync(PcBuildInputDto buildDto, Guid userId);
        Task<bool> DeleteBuildAsync(Guid pcBuildId, Guid userId);
        Task<PcBuildRequestDto?> GetBuildByIdAsync(Guid pcBuildId);
        Task<List<PcBuild>> GetAllBuildsAsync();
        Task<List<PcBuildListDto>> GetUserBuildsAsync(Guid userId);
        Task<bool> UpdateBuildAsync(PcBuild pcBuild);
        Task<List<CompatibilityResult>> CheckComponentsCompatibilityAsync(ComponentsCompatibilityDto dto);
        
    }
}
