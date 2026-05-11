using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCommentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var review = await _context.Set<Review>()
                .FirstOrDefaultAsync(r => r.Id == request.CommentId, cancellationToken);

            if (review == null)
                return Result.Failure<bool>(new Error("NotFound", "Review not found.", 404));

            if (review.UserId != request.UserId)
                return Result.Failure<bool>(new Error("Forbidden", "You can only delete your own reviews.", 403));

            var buildId = review.PcBuildId;

            _context.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);

            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == buildId, cancellationToken);

            if (build != null)
            {
                var reviews = await _context.Set<Review>()
                    .Where(r => r.PcBuildId == buildId)
                    .ToListAsync(cancellationToken);

                build.AverageRating = reviews.Count > 0
                    ? Math.Round(reviews.Average(r => r.Rating), 2)
                    : 0;

                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success(true);
        }
    }
}