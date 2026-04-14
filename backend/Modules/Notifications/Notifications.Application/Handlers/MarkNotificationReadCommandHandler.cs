using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Commands;
using Notifications.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Notifications.Application.Handlers
{
    public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public MarkNotificationReadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _context.Set<Notification>()
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

            if (notification == null)
                return Result.Failure<bool>(new Error("NotFound", "Notification not found.", 404));

            notification.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(true);
        }
    }
}