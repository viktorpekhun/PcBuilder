using Components.Application.Commands;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Components.Application.Handlers
{
    public class DeletePriceAlertCommandHandler : IRequestHandler<DeletePriceAlertCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeletePriceAlertCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeletePriceAlertCommand request, CancellationToken cancellationToken)
        {
            var subscription = await _context.Set<PriceAlertSubscription>()
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (subscription is null)
                return Result.Failure<bool>(new Error("NotFound", "Price alert not found.", 404));

            if (subscription.UserId != request.UserId)
                return Result.Failure<bool>(new Error("Forbidden", "You do not have permission to delete this alert.", 403));

            _context.Set<PriceAlertSubscription>().Remove(subscription);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(true);
        }
    }
}
