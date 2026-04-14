using MediatR;
using Moderation.Application.Dtos;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Queries
{
    public record GetUserBansQuery(Guid UserId) : IRequest<Result<UserBanStatusDto>>;
}