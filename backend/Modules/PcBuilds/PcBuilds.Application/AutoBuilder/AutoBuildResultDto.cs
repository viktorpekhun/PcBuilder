using PcBuilds.Application.Compatibility;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.AutoBuilder
{
    public record AutoBuildResultDto(
        PcBuildGalleryDto Build,
        decimal TotalPrice,
        double FitnessScore,
        BuildCompatibilityReport CompatibilityReport);
}
