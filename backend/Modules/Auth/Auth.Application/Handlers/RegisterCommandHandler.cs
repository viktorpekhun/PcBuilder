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
        private readonly IEmailService _emailService;

        public RegisterCommandHandler(
            IApplicationDbContext context,
            ITokenProviderService tokenProviderService,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _tokenProviderService = tokenProviderService;
            _configuration = configuration;
            _emailService = emailService;
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

            var userRole = await _context.Set<Role>()
                .FirstAsync(r => r.Name == "User", cancellationToken);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                CommentBanUntil = DateTime.MinValue,
                PostBanUntil = DateTime.MinValue,
                IsEmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString("N"),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            user.Roles.Add(userRole);

            var refreshToken = _tokenProviderService.CreateRefreshToken();
            user.RefreshToken = refreshToken;

            int expirationDays = _configuration.GetValue<int>("Jwt:ExpirationInDays");
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(expirationDays);

            await _context.Set<User>().AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Generate access token after save so user.Id is assigned by EF
            var accessToken = _tokenProviderService.CreateAccessToken(user);

            await _emailService.SendVerificationEmailAsync(user.Email, user.EmailVerificationToken, cancellationToken);

            return Result<AuthResultDto>.Success(new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpirationDays = expirationDays
            });
        }
    }
}
