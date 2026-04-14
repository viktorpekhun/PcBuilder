using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class PublishPcBuildCommandHandler : IRequestHandler<PublishPcBuildCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public PublishPcBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(PublishPcBuildCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

            if (build == null)
                return Result.Failure<bool>(new Error("NotFound", "Build not found.", 404));

            if (build.UserId != request.UserId)
                return Result.Failure<bool>(new Error("Forbidden", "You do not have permission to publish this build.", 403));

            if (request.IsPublished)
            {
                var user = await _context.Set<User>()
                    .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                if (user != null && user.PostBanUntil > DateTime.UtcNow)
                    return Result.Failure<bool>(new Error("Forbidden", "You are temporarily banned from posting.", 403));
            }

            build.IsPublished = request.IsPublished;
            build.PublishedAt = request.IsPublished ? DateTime.UtcNow : null;
            build.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(request.IsPublished);
        }
    }
}