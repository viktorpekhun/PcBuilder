using MediatR;
using Moderation.Application.Dtos;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Queries
{
    public record GetAdminStatsQuery : IRequest<Result<AdminStatsDto>>;
}