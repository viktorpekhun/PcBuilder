using MediatR;
using PcBuilder.SharedKernel;

namespace Notifications.Application.Queries
{
    public record GetUnreadCountQuery(Guid UserId) : IRequest<Result<int>>;
}