using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Commands;
using Notifications.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Notifications.Application.Handlers
{
    public class MarkAllReadCommandHandler : IRequestHandler<MarkAllReadCommand, Result<int>>
    {
        private readonly IApplicationDbContext _context;

        public MarkAllReadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(MarkAllReadCommand request, CancellationToken cancellationToken)
        {
            var unread = await _context.Set<Notification>()
                .Where(n => n.UserId == request.UserId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in unread)
                notification.IsRead = true;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(unread.Count);
        }
    }
}