using MediatR;
using Moderation.Application.Dtos;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;

namespace Moderation.Application.Queries
{
    public record GetAdminActivityQuery(
        int DaysBack,
        int PageNumber,
        int PageSize) : IRequest<Result<PagedResponse<AdminActivityLogDto>>>;
}
