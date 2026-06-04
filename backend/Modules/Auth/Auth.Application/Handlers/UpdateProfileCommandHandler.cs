using Auth.Application.Commands;
using Auth.Application.Dtos;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<ProfileDto>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProfileCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<ProfileDto>.Failure(new Error("NotFound", "User not found.", 404));

            if (!string.Equals(user.Username, request.Username, StringComparison.OrdinalIgnoreCase))
            {
                var taken = await _context.Set<User>()
                    .AnyAsync(u => u.Username == request.Username && u.Id != request.UserId, cancellationToken);

                if (taken)
                    return Result<ProfileDto>.Failure(new Error("UsernameTaken", "This username is already taken.", 409));
            }

            user.Username = request.Username;
            user.Bio = request.Bio;

            if (request.PreferredLanguage is "en" or "uk")
                user.PreferredLanguage = request.PreferredLanguage;

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ProfileDto>.Success(new ProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                EmailVerified = user.IsEmailVerified,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                PreferredLanguage = user.PreferredLanguage,
                HasPassword = !string.IsNullOrEmpty(user.PasswordHash),
                GoogleLinked = !string.IsNullOrEmpty(user.GoogleId),
                Roles = user.Roles.Select(r => r.Name).ToList(),
                CreatedAt = user.CreatedAt,
            });
        }
    }
}
