using Auth.Application.Commands;
using Auth.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PcBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
        {
            var result = await _mediator.Send(new RegisterCommand(request.Username, request.Email, request.Password));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            SetRefreshTokenCookie(result.Value!.RefreshToken, result.Value.RefreshTokenExpirationDays);
            return Ok(new TokenResponseDto { AccessToken = result.Value.AccessToken });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            var result = await _mediator.Send(new LoginCommand(request.Email, request.Password));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            SetRefreshTokenCookie(result.Value!.RefreshToken, result.Value.RefreshTokenExpirationDays);
            return Ok(new TokenResponseDto { AccessToken = result.Value.AccessToken });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto request)
        {
            var result = await _mediator.Send(new GoogleLoginCommand(request.IdToken));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            SetRefreshTokenCookie(result.Value!.RefreshToken, result.Value.RefreshTokenExpirationDays);
            return Ok(new TokenResponseDto { AccessToken = result.Value.AccessToken });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            ClearRefreshTokenCookie();
            await _mediator.Send(new LogoutCommand(refreshToken));
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null)
                return Unauthorized(new { Message = "No refresh token provided" });

            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken));
            if (result.IsFailure)
            {
                ClearRefreshTokenCookie();
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });
            }

            SetRefreshTokenCookie(result.Value!.RefreshToken, result.Value.RefreshTokenExpirationDays);
            return Ok(new TokenResponseDto { AccessToken = result.Value.AccessToken });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var result = await _mediator.Send(new VerifyEmailCommand(token));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Message = result.Value });
        }

        [Authorize]
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail()
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new ResendVerificationEmailCommand(userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Message = result.Value });
        }

        private Guid GetUserId()
        {
            var idStr = User.FindFirst("sub")?.Value
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(idStr, out var guid))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return guid;
        }

        private void SetRefreshTokenCookie(string refreshToken, int expirationDays)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(expirationDays)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(-1)
            });
        }
    }
}
