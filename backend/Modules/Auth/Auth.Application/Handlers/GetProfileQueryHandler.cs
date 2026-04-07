using Auth.Application.Dtos;
using Auth.Application.Queries;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<ProfileDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetProfileQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                return Result<ProfileDto>.Failure(new Error("NotFound", "User not found.", 404));

            return Result<ProfileDto>.Success(new ProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                EmailVerified = user.IsEmailVerified,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                HasPassword = !string.IsNullOrEmpty(user.PasswordHash),
                GoogleLinked = !string.IsNullOrEmpty(user.GoogleId),
                Roles = user.Roles.Select(r => r.Name).ToList(),
                CreatedAt = user.CreatedAt,
            });
        }
    }
}
