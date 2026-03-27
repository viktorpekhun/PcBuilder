using Auth.Application.Commands;
using Auth.Application.Dtos;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Exceptions;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITokenProviderService _tokenProviderService;
        private readonly IConfiguration _configuration;

        public RegisterCommandHandler(
            IApplicationDbContext context,
            ITokenProviderService tokenProviderService,
            IConfiguration configuration)
        {
            _context = context;
            _tokenProviderService = tokenProviderService;
            _configuration = configuration;
        }

        public async Task<Result<AuthResultDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                throw new ConflictException("Email is already taken.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                CommentBanUntil = DateTime.MinValue,
                PostBanUntil = DateTime.MinValue
            };

            var accessToken = _tokenProviderService.CreateAccessToken(user);
            var refreshToken = _tokenProviderService.CreateRefreshToken();
            user.RefreshToken = refreshToken;

            int expirationDays = _configuration.GetValue<int>("Jwt:ExpirationInDays");
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(expirationDays);

            await _context.Set<User>().AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<AuthResultDto>.Success(new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpirationDays = expirationDays
            });
        }
    }
}
