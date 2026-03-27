using MediatR;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Queries
{
    public record GetUserBuildsQuery(Guid UserId) : IRequest<List<PcBuildListDto>>;
}
