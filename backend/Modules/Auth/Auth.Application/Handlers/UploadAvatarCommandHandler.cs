using Auth.Application.Commands;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public UploadAvatarCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<string>.Failure(new Error("NotFound", "User not found.", 404));

            var avatarUrl = await _fileStorage.SaveAvatarAsync(request.UserId, request.Data, cancellationToken);
            user.AvatarUrl = avatarUrl;

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(user.AvatarUrl);
        }
    }
}
