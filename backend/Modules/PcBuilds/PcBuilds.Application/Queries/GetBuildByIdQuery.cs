using MediatR;
using PcBuilder.SharedKernel;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Queries
{
    public record GetBuildByIdQuery(Guid PcBuildId) : IRequest<Result<PcBuildRequestDto>>;
}