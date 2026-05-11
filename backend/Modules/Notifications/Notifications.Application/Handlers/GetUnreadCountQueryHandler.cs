using MediatR;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Queries;
using Notifications.Domain.Entities;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Notifications.Application.Handlers
{
    public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, Result<int>>
    {
        private readonly IApplicationDbContext _context;

        public GetUnreadCountQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _context.Set<Notification>()
                .CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);

            return Result.Success(count);
        }
    }
}