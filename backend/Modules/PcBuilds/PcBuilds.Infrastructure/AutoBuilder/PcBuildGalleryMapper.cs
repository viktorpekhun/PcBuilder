using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.Dtos;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Infrastructure.AutoBuilder
{
    public class PcBuildGalleryMapper : IPcBuildGalleryMapper
    {
        public PcBuildGalleryDto ToGalleryDto(PcBuild build)
        {
            return new PcBuildGalleryDto
            {
                Id = build.Id,
                Name = build.Name,
                Description = build.Description,
                Price = build.Price,
                AverageRating = build.AverageRating,
                PublishedAt = build.PublishedAt,
                Username = build.User?.Username ?? string.Empty,
                AvatarUrl = build.User?.AvatarUrl,
                ComponentCount =
                    (build.CpuId != null ? 1 : 0) +
                    (build.GpuId != null ? 1 : 0) +
                    (build.MotherboardId != null ? 1 : 0) +
                    (build.CpuCoolerId != null ? 1 : 0) +
                    (build.PowerSupplyId != null ? 1 : 0) +
                    (build.PcCaseId != null ? 1 : 0) +
                    build.PcBuild_Rams.Count +
                    build.PcBuild_Ssds.Count +
                    build.PcBuild_Hdds.Count +
                    build.PcBuild_Fans.Count,
                CommentCount = build.Reviews?.Count ?? 0,
            };
        }
    }
}
