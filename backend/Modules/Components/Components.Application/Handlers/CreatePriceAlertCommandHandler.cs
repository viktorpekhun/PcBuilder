using Components.Application.Commands;
using Components.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Components.Application.Handlers
{
    public class CreatePriceAlertCommandHandler : IRequestHandler<CreatePriceAlertCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public CreatePriceAlertCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(CreatePriceAlertCommand request, CancellationToken cancellationToken)
        {
            if (request.ThresholdPercent <= 0 || request.ThresholdPercent > 100)
                return Result.Failure<Guid>(new Error("InvalidThreshold", "Threshold must be between 0 and 100 percent.", 400));

            var latestEntry = await _context.Set<PriceHistoryEntry>()
                .Where(p => p.ComponentId == request.ComponentId && p.ComponentType == request.ComponentType)
                .OrderByDescending(p => p.SnapshotDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestEntry is null)
                return Result.Failure<Guid>(new Error("NoPriceData", "No price data is available for this component yet.", 400));

            var existing = await _context.Set<PriceAlertSubscription>()
                .FirstOrDefaultAsync(
                    s => s.UserId == request.UserId
                         && s.ComponentId == request.ComponentId
                         && s.ComponentType == request.ComponentType,
                    cancellationToken);

            if (existing is not null)
            {
                existing.ThresholdPercent = request.ThresholdPercent;
                existing.LastNotifiedPrice = latestEntry.AveragePrice;
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success(existing.Id);
            }

            var subscription = new PriceAlertSubscription
            {
                UserId = request.UserId,
                ComponentId = request.ComponentId,
                ComponentType = request.ComponentType,
                ThresholdPercent = request.ThresholdPercent,
                InitialPrice = latestEntry.AveragePrice,
                LastNotifiedPrice = latestEntry.AveragePrice,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Set<PriceAlertSubscription>().AddAsync(subscription, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(subscription.Id);
        }
    }
}
