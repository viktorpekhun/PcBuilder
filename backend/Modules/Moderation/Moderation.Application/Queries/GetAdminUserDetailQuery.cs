using MediatR;
using Moderation.Application.Dtos;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Queries
{
    public record GetAdminUserDetailQuery(Guid UserId) : IRequest<Result<AdminUserDetailDto>>;
}