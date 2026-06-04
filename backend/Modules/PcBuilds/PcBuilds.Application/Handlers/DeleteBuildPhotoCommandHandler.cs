using Auth.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class DeleteBuildPhotoCommandHandler : IRequestHandler<DeleteBuildPhotoCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public DeleteBuildPhotoCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Result<bool>> Handle(DeleteBuildPhotoCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.BuildId, cancellationToken);

            if (build == null)
                return Result.Failure<bool>(new Error("NotFound", "Build not found.", 404));

            if (build.UserId != request.UserId)
                return Result.Failure<bool>(new Error("Forbidden", "You do not have permission to modify this build.", 403));

            if (build.PhotoUrl == null)
                return Result.Failure<bool>(new Error("NotFound", "This build has no photo.", 404));

            await _fileStorage.DeleteBuildPhotoAsync(request.BuildId, cancellationToken);

            build.PhotoUrl = null;
            build.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(true);
        }
    }
}
