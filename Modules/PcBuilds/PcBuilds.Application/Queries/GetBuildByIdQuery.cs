using MediatR;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Queries
{
    public record GetBuildByIdQuery(Guid PcBuildId) : IRequest<PcBuildRequestDto?>;
}
