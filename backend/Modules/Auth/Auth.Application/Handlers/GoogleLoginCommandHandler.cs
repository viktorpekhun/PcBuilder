using Auth.Application.Commands;
using Auth.Application.Dtos;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Persistence;

namespace Auth.Application.Handlers
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITokenProviderService _tokenProviderService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IConfiguration _configuration;

        public GoogleLoginCommandHandler(
            IApplicationDbContext context,
            ITokenProviderService tokenProviderService,
            IGoogleAuthService googleAuthService,
            IConfiguration configuration)
        {
            _context = context;
            _tokenProviderService = tokenProviderService;
            _googleAuthService = googleAuthService;
            _configuration = configuration;
        }

        public async Task<Result<AuthResultDto>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            GoogleUserInfo googleUser;
            try
            {
                googleUser = await _googleAuthService.VerifyIdTokenAsync(request.IdToken);
            }
            catch
            {
                return Result<AuthResultDto>.Failure(
                    new Error("InvalidGoogleToken", "Invalid or expired Google token.", 401));
            }

            var user = await _context.Set<User>()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.GoogleId == googleUser.GoogleId, cancellationToken);

            if (user == null)
            {
                // Check if a local account with the same email exists — link it
                user = await _context.Set<User>()
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Email == googleUser.Email, cancellationToken);

                if (user != null)
                {
                    // Link Google to existing local account
                    user.GoogleId = googleUser.GoogleId;
                }
                else
                {
                    // Create new user from Google account
                    var userRole = await _context.Set<Role>()
                        .FirstAsync(r => r.Name == "User", cancellationToken);

                    var username = GenerateUsername(googleUser.Name, googleUser.Email);

                    user = new User
                    {
                        Username = username,
                        Email = googleUser.Email,
                        GoogleId = googleUser.GoogleId,
                        PasswordHash = null,
                        IsEmailVerified = true,
                        CommentBanUntil = DateTime.MinValue,
                        PostBanUntil = DateTime.MinValue
                    };

                    user.Roles.Add(userRole);
                    await _context.Set<User>().AddAsync(user, cancellationToken);
                }
            }

            var accessToken = _tokenProviderService.CreateAccessToken(user);
            var refreshToken = _tokenProviderService.CreateRefreshToken();

            int expirationDays = _configuration.GetValue<int>("Jwt:ExpirationInDays");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(expirationDays);

            await _context.SaveChangesAsync(cancellationToken);

            return Result<AuthResultDto>.Success(new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpirationDays = expirationDays
            });
        }

        private static string GenerateUsername(string name, string email)
        {
            // Derive from name, fallback to email prefix
            var source = string.IsNullOrWhiteSpace(name) ? email.Split('@')[0] : name;

            // Keep only alphanumeric, underscores, hyphens
            var sanitized = new string(source
                .Replace(" ", "_")
                .Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
                .ToArray());

            if (sanitized.Length == 0)
                sanitized = "user";

            // Truncate to leave room for suffix (max 30 chars total)
            if (sanitized.Length > 22)
                sanitized = sanitized[..22];

            // Append random suffix to reduce collision chance
            var suffix = Random.Shared.Next(1000, 9999).ToString();
            return $"{sanitized}_{suffix}";
        }
    }
}
