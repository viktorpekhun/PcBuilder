using PcBuilderApi.Models;
using System.Security.Claims;

namespace PcBuilderApi.Services.Interfaces
{
    public interface ITokenProviderService
    {
        string CreateAccessToken(User user);
        string CreateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
