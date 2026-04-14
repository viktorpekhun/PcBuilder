using MediatR;
using PcBuilder.SharedKernel;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Queries
{
    public record GetUserBuildsQuery(Guid UserId) : IRequest<Result<List<PcBuildListDto>>>;
}