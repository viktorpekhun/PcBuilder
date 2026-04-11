using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Exceptions;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCommentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var review = await _context.Set<Review>()
                .FirstOrDefaultAsync(r => r.Id == request.CommentId, cancellationToken);

            if (review == null)
                throw new NotFoundException("Review not found.");

            if (review.UserId != request.UserId)
                throw new ForbiddenException("You can only delete your own reviews.");

            var buildId = review.PcBuildId;

            _context.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);

            // Recalculate average rating
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

            return true;
        }
    }
}
