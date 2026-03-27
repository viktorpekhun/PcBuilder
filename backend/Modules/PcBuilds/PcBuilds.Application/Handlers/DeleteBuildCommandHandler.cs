using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class DeleteBuildCommandHandler : IRequestHandler<DeleteBuildCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public DeleteBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteBuildCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var build = await _context.Set<PcBuild>()
                    .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

                if (build == null)
                    return false;

                if (build.UserId != request.UserId)
                    return false;

                _context.Set<PcBuild>().Remove(build);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
