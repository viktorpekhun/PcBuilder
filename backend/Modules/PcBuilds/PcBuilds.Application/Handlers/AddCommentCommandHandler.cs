using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Exceptions;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public AddCommentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

            if (build == null)
                throw new NotFoundException("Build not found.");

            if (!build.IsPublished && build.UserId != request.UserId)
                throw new ForbiddenException("You cannot comment on this build.");

            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                throw new NotFoundException("User not found.");

            if (user.CommentBanUntil > DateTime.UtcNow)
                throw new ForbiddenException("You are temporarily banned from commenting.");

            var review = new Review
            {
                Text = request.Text,
                Rating = request.Rating,
                UserId = request.UserId,
                PcBuildId = request.PcBuildId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Set<Review>().AddAsync(review, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Recalculate average rating
            var avgRating = await _context.Set<Review>()
                .Where(r => r.PcBuildId == request.PcBuildId)
                .AverageAsync(r => r.Rating, cancellationToken);

            build.AverageRating = Math.Round(avgRating, 2);
            await _context.SaveChangesAsync(cancellationToken);

            return review.Id;
        }
    }
}
