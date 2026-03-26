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
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ITokenProviderService _tokenProviderService;
        private readonly IConfiguration _configuration;

        public RefreshTokenCommandHandler(
            IApplicationDbContext context,
            ITokenProviderService tokenProviderService,
            IConfiguration configuration)
        {
            _context = context;
            _tokenProviderService = tokenProviderService;
            _configuration = configuration;
        }

        public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return Result<AuthResultDto>.Failure(
                    new Error("MissingToken", "Refresh token is required.", 401));
            }

            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user == null)
            {
                return Result<AuthResultDto>.Failure(
                    new Error("InvalidToken", "Invalid refresh token.", 403));
            }

            if (user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return Result<AuthResultDto>.Failure(
                    new Error("ExpiredToken", "Refresh token has expired.", 401));
            }

            var newAccessToken = _tokenProviderService.CreateAccessToken(user);
            var newRefreshToken = _tokenProviderService.CreateRefreshToken();

            int expirationDays = _configuration.GetValue<int>("Jwt:ExpirationInDays");
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(expirationDays);

            _context.Set<User>().Update(user);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<AuthResultDto>.Success(new AuthResultDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpirationDays = expirationDays
            });
        }
    }
}
