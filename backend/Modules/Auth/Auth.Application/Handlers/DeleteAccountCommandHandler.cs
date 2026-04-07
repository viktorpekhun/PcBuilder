using Auth.Application.Commands;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Result<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public DeleteAccountCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Result<string>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<string>.Failure(new Error("NotFound", "User not found.", 404));

            await _fileStorage.DeleteAvatarAsync(request.UserId, cancellationToken);

            _context.Set<User>().Remove(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Account deleted successfully.");
        }
    }
}
