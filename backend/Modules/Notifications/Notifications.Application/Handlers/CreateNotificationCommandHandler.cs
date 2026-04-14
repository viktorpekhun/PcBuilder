using System.Text.Json;
using MediatR;
using Notifications.Application.Commands;
using Notifications.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Notifications.Application.Handlers
{
    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public CreateNotificationCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                UserId = request.UserId,
                Type = request.Type,
                Payload = JsonSerializer.Serialize(request.Payload),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Set<Notification>().AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(notification.Id);
        }
    }
}