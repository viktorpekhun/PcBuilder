using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class DeleteBuildCommandHandler : IRequestHandler<DeleteBuildCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteBuildCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(DeleteBuildCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.PcBuildId, cancellationToken);

            if (build == null)
                return Result.Failure<bool>(new Error("NotFound", "Build not found.", 404));

            if (build.UserId != request.UserId)
                return Result.Failure<bool>(new Error("Forbidden", "You do not have permission to delete this build.", 403));

            _context.Set<PcBuild>().Remove(build);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(true);
        }
    }
}