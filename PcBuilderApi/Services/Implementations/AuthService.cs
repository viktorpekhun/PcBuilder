using Microsoft.AspNetCore.Http;
using PcBuilderApi.Dtos.UserDtos;
using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Services.Interfaces;

namespace PcBuilderApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenProviderService _tokenProviderService;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUnitOfWork unitOfWork,
            ITokenProviderService tokenProviderService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _tokenProviderService = tokenProviderService;
            _configuration = configuration;
        }

        public async Task<(TokenResponseDto? TokenResponse, string? ErrorMessage, int StatusCode)> RegisterAsync(UserRegisterDto request, HttpResponse response)
        {
            try
            {
                var existingUser = await _unitOfWork.Repository<User>().GetFirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return (null, "Email is already taken.", StatusCodes.Status409Conflict);
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

                // Set the refresh token cookie
                SetRefreshTokenCookie(response, refreshToken, expirationDays);

                await _unitOfWork.Repository<User>().AddAsync(user);
                await _unitOfWork.SaveAsync();

                return (new TokenResponseDto { AccessToken = accessToken }, null, StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                // Log exception details here
                return (null, "An error occurred while processing your request.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<(TokenResponseDto? TokenResponse, string? ErrorMessage, int StatusCode)> LoginAsync(UserLoginDto request, HttpResponse response)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return (null, "Email and password are required.", StatusCodes.Status400BadRequest);
                }

                var user = await _unitOfWork.Repository<User>().GetFirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return (null, "Invalid email or password.", StatusCodes.Status401Unauthorized);
                }

                var accessToken = _tokenProviderService.CreateAccessToken(user);
                var refreshToken = _tokenProviderService.CreateRefreshToken();

                int expirationDays = _configuration.GetValue<int>("Jwt:ExpirationInDays");
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(expirationDays);

                // Set the refresh token cookie
                SetRefreshTokenCookie(response, refreshToken, expirationDays);

                await _unitOfWork.Repository<User>().UpdateAsync(user);
                await _unitOfWork.SaveAsync();

                return (new TokenResponseDto { AccessToken = accessToken }, null, StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                // Log exception details here
                return (null, "An error occurred while processing your request.", StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> LogoutAsync(string? refreshToken, HttpResponse response)
        {
            // Clear the refresh token cookie regardless of backend success
            ClearRefreshTokenCookie(response);

            if (string.IsNullOrEmpty(refreshToken))
            {
                return (true, null); // Nothing to do on the server side
            }

            try
            {
                var user = await _unitOfWork.Repository<User>().GetFirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
                if (user != null)
                {
                    user.RefreshToken = "";
                    user.RefreshTokenExpiryTime = DateTime.MinValue;
                    await _unitOfWork.Repository<User>().UpdateAsync(user);
                    await _unitOfWork.SaveAsync();
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                // Log exception details here
                return (false, "An error occurred while logging out.");
            }
        }

        public async Task<(TokenResponseDto? TokenResponse, string? ErrorMessage, int StatusCode)> RefreshTokenAsync(
            string refreshToken, HttpResponse response)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    ClearRefreshTokenCookie(response);
                    return (null, "Refresh token is required.", StatusCodes.Status401Unauthorized);
                }

                var user = await _unitOfWork.Repository<User>().GetFirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
                if (user == null)
                {
                    ClearRefreshTokenCookie(response);
                    return (null, "Invalid refresh token.", StatusCodes.Status403Forbidden);
                }

                if (user.RefreshTokenExpiryTime <= DateTime.Now)
                {
                    ClearRefreshTokenCookie(response);
                    return (null, "Refresh token has expired.", StatusCodes.Status401Unauthorized);
                }

                var newAccessToken = _tokenProviderService.CreateAccessToken(user);
                var newRefreshToken = _tokenProviderService.CreateRefreshToken();

                int expirationDays = _configuration.GetValue<int>("Jwt:ExpirationInDays");
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(expirationDays);

                SetRefreshTokenCookie(response, newRefreshToken, expirationDays);

                await _unitOfWork.Repository<User>().UpdateAsync(user);
                await _unitOfWork.SaveAsync();

                return (new TokenResponseDto { AccessToken = newAccessToken }, null, StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                // Log exception details here
                return (null, "An error occurred while refreshing the token.", StatusCodes.Status500InternalServerError);
            }
        }

        private void SetRefreshTokenCookie(HttpResponse response, string refreshToken, int expirationDays)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(expirationDays)
            };

            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private void ClearRefreshTokenCookie(HttpResponse response)
        {
            response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(-1)
            });
        }
    }
}