using Auth.Application.Dtos;
using Auth.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly string _clientId;

        public GoogleAuthService(IConfiguration configuration)
        {
            _clientId = configuration["Google:ClientId"]!;
        }

        public async Task<GoogleUserInfo> VerifyIdTokenAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_clientId]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserInfo
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                EmailVerified = payload.EmailVerified
            };
        }
    }
}
