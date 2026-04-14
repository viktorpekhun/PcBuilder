using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Queries
{
    public record GetPublicBuildsQuery(ResourceParameters Parameters) : IRequest<Result<PagedResponse<PcBuildGalleryDto>>>;
}