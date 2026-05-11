using MediatR;
using Moderation.Application.Dtos;
using PcBuilder.SharedKernel;

namespace Moderation.Application.Queries
{
    public record GetUserWarningsQuery(Guid UserId) : IRequest<Result<List<WarningDto>>>;
}