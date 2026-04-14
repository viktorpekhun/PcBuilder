using MediatR;
using Notifications.Application.Dtos;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;

namespace Notifications.Application.Queries
{
    public record GetNotificationsQuery(
        Guid UserId,
        bool? OnlyUnread,
        int PageNumber = 1,
        int PageSize = 10) : IRequest<Result<PagedResponse<NotificationDto>>>;
}