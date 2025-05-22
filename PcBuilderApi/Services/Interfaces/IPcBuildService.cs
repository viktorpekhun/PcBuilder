using PcBuilderApi.Dtos.PcBuildDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Services.Compatibility;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Interfaces
{
    public interface IPcBuildService
    {
        Task<bool> SaveBuildAsync(Guid userId, PcBuildInputDto buildDto);
        Task<bool> UpdateBuildAsync(Guid pcBuildId, PcBuildInputDto buildDto);
        Task<bool> DeleteBuildAsync(Guid pcBuildId, Guid userId);
        Task<PcBuildRequestDto?> GetBuildByIdAsync(Guid pcBuildId);
        Task<List<PcBuild>> GetAllBuildsAsync();
        Task<List<PcBuildListDto>> GetUserBuildsAsync(Guid userId);
        Task<List<CompatibilityResult>> CheckComponentsCompatibilityAsync(ComponentsCompatibilityDto dto);
        
    }
}
