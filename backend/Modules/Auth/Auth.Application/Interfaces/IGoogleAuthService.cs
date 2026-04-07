using Auth.Application.Dtos;

namespace Auth.Application.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken);
    }
}
