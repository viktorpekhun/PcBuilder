using MediatR;
using Microsoft.EntityFrameworkCore;
using Moderation.Application.Commands;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Domain.Entities;

namespace Moderation.Application.Handlers
{
    public class ReportReviewCommandHandler : IRequestHandler<ReportReviewCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public ReportReviewCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(ReportReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _context.Set<Review>()
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

            if (review == null)
                return Result.Failure<Guid>(new Error("NotFound", "Review not found.", 404));

            if (review.UserId == request.ReporterId)
                return Result.Failure<Guid>(new Error("Conflict", "You cannot report your own review.", 409));

            var alreadyReported = await _context.Set<Report>()
                .AnyAsync(r => r.ReporterId == request.ReporterId
                    && r.ReportType == ReportType.Review
                    && r.ReportedEntityId == request.ReviewId
                    && r.Status == ReportStatus.Pending, cancellationToken);

            if (alreadyReported)
                return Result.Failure<Guid>(new Error("Conflict", "You have already reported this review.", 409));

            var report = new Report
            {
                ReporterId = request.ReporterId,
                ReportType = ReportType.Review,
                ReportedEntityId = request.ReviewId,
                ReportedUserId = review.UserId,
                Reason = request.Reason,
                Status = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Set<Report>().AddAsync(report, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(report.Id);
        }
    }
}