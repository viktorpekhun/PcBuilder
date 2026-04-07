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
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITokenProviderService _tokenProviderService;
        private readonly IConfiguration _configuration;

        public LoginCommandHandler(
            IApplicationDbContext context,
            ITokenProviderService tokenProviderService,
            IConfiguration configuration)
        {
            _context = context;
            _tokenProviderService = tokenProviderService;
            _configuration = configuration;
        }

        public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<User>()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Result<AuthResultDto>.Failure(
                    new Error("InvalidCredentials", "Invalid email or password.", 401));
            }

            var accessToken = _tokenProviderService.CreateAccessToken(user);
            var refreshToken = _tokenProviderService.CreateRefreshToken();

            int expirationDays = _configuration.GetValue<int>("Jwt:ExpirationInDays");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(expirationDays);

            _context.Set<User>().Update(user);
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
