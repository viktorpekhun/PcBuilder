using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Exceptions;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class PublishPcBuildCommandHandler : IRequestHandler<PublishPcBuildCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public PublishPcBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(PublishPcBuildCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

            if (build == null)
                throw new NotFoundException("Build not found.");

            if (build.UserId != request.UserId)
                throw new ForbiddenException("You do not have permission to publish this build.");

            if (request.IsPublished)
            {
                build.IsPublished = true;
                build.PublishedAt = DateTime.UtcNow;
            }
            else
            {
                build.IsPublished = false;
                build.PublishedAt = null;
            }

            build.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return request.IsPublished;
        }
    }
}
