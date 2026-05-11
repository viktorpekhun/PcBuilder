using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Dtos;
using Notifications.Application.Queries;
using Notifications.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Filtering;
using PcBuilder.SharedKernel.Persistence;

namespace Notifications.Application.Handlers
{
    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<PagedResponse<NotificationDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetNotificationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedResponse<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Set<Notification>()
                .Where(n => n.UserId == request.UserId);

            if (request.OnlyUnread == true)
                query = query.Where(n => !n.IsRead);

            var totalCount = await query.CountAsync(cancellationToken);

            var entities = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = entities.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Payload = JsonSerializer.Deserialize<Dictionary<string, string>>(n.Payload) ?? new(),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();

            var parameters = new ResourceParameters
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(new PagedResponse<NotificationDto>(items, totalCount, parameters));
        }
    }
}