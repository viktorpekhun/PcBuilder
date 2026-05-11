using MediatR;
using Moderation.Application.Dtos;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;

namespace Moderation.Application.Queries
{
    public record GetAdminUsersQuery(
        string? SearchQuery,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<Result<PagedResponse<AdminUserDto>>>;
}