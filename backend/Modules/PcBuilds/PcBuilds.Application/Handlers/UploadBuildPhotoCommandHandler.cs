using Auth.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;
using PcBuilds.Application.Commands;
using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Handlers
{
    public class UploadBuildPhotoCommandHandler : IRequestHandler<UploadBuildPhotoCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public UploadBuildPhotoCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Result<string>> Handle(UploadBuildPhotoCommand request, CancellationToken cancellationToken)
        {
            var build = await _context.Set<PcBuild>()
                .FirstOrDefaultAsync(b => b.Id == request.BuildId, cancellationToken);

            if (build == null)
                return Result.Failure<string>(new Error("NotFound", "Build not found.", 404));

            if (build.UserId != request.UserId)
                return Result.Failure<string>(new Error("Forbidden", "You do not have permission to modify this build.", 403));

            var url = await _fileStorage.SaveBuildPhotoAsync(request.BuildId, request.Data, cancellationToken);

            build.PhotoUrl = url;
            build.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(url);
        }
    }
}
