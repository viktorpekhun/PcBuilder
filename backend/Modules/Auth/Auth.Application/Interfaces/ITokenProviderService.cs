using Auth.Domain.Entities;
using System.Security.Claims;

namespace Auth.Application.Interfaces
{
    public interface ITokenProviderService
    {
        string CreateAccessToken(User user);
        string CreateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
