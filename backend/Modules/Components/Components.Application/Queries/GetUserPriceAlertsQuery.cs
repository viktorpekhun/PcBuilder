using Components.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;

namespace Components.Application.Queries
{
    public record GetUserPriceAlertsQuery(Guid UserId) : IRequest<Result<List<UserPriceAlertDto>>>;
}
