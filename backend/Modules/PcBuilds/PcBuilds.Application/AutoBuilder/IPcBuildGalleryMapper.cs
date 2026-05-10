using PcBuilds.Application.Dtos;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.AutoBuilder
{
    public interface IPcBuildGalleryMapper
    {
        PcBuildGalleryDto ToGalleryDto(PcBuild build);
    }
}
