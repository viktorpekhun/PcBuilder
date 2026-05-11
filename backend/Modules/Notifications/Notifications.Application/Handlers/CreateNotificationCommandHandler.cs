using System.Text.Json;
using MediatR;
using Notifications.Application.Commands;
using Notifications.Application.Events;
using Notifications.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Notifications.Application.Handlers
{
    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPublisher _publisher;

        public CreateNotificationCommandHandler(IApplicationDbContext context, IPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Serialize(request.Payload);
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = request.Type,
                Payload = payload,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Set<Notification>().AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new NotificationCreatedEvent(
                notification.Id,
                notification.UserId,
                notification.Type,
                payload,
                notification.CreatedAt), cancellationToken);

            return Result.Success(notification.Id);
        }
    }
}