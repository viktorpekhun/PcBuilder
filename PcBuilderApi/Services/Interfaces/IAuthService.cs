using PcBuilderApi.Dtos.UserDtos;

namespace PcBuilderApi.Services.Interfaces
{

    public interface IAuthService
    {
        Task<(TokenResponseDto? TokenResponse, string? ErrorMessage, int StatusCode)> RegisterAsync(UserRegisterDto request, HttpResponse response);
        Task<(TokenResponseDto? TokenResponse, string? ErrorMessage, int StatusCode)> LoginAsync(UserLoginDto request, HttpResponse response);
        Task<(bool Success, string? ErrorMessage)> LogoutAsync(string? refreshToken, HttpResponse response);
        Task<(TokenResponseDto? TokenResponse, string? ErrorMessage, int StatusCode)> RefreshTokenAsync(string refreshToken, HttpResponse response);
    }
    
}
