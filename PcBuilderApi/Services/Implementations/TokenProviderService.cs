using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PcBuilderApi.Models;
using PcBuilderApi.Services.Interfaces;
using System.Security.Claims;
using System.Text;

namespace PcBuilderApi.Services.Implementations
{
    public class TokenProviderService : ITokenProviderService
    {
        private readonly IConfiguration _configuration;

        public TokenProviderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateAccessToken(User user)
        {
            string secretKey = _configuration["Jwt:Secret"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                        new Claim(JwtRegisteredClaimNames.Name, user.Username),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
                    ]),
                Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var handler = new JsonWebTokenHandler();
            string token = handler.CreateToken(tokenDescriptor);
            return token;
        }

        public string CreateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JsonWebTokenHandler();
            var resultTask = tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
            var result = resultTask.GetAwaiter().GetResult();

            if (!result.IsValid)
            {
                throw new UnauthorizedAccessException("Invalid access token");
            }
            return new ClaimsPrincipal(result.ClaimsIdentity);
        }
    }
}
