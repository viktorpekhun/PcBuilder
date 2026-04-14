using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;
using PcBuilds.Application.Dtos;

namespace PcBuilds.Application.Queries
{
    public record GetBuildCommentsQuery(Guid PcBuildId, int PageNumber, int PageSize) : IRequest<Result<PagedResponse<CommentDto>>>;
}