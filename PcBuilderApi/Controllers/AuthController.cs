using Microsoft.AspNetCore.Mvc;
using PcBuilderApi.Dtos.UserDtos;
using PcBuilderApi.Services.Interfaces;

namespace PcBuilderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<TokenResponseDto>> Register([FromBody] UserRegisterDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (tokenResponse, errorMessage, statusCode) = await _authService.RegisterAsync(request, Response);

            if (tokenResponse == null)
            {
                return StatusCode(statusCode, new { Message = errorMessage });
            }

            return Ok(tokenResponse);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] UserLoginDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (tokenResponse, errorMessage, statusCode) = await _authService.LoginAsync(request, Response);

            if (tokenResponse == null)
            {
                return StatusCode(statusCode, new { Message = errorMessage });
            }

            return Ok(tokenResponse);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var (success, errorMessage) = await _authService.LogoutAsync(refreshToken, Response);

            if (!success)
            {
                return StatusCode(500, new { Message = errorMessage });
            }

            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpGet("refresh")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null)
            {
                return Unauthorized(new { Message = "No refresh token provided" });
            }

            var (tokenResponse, errorMessage, statusCode) = await _authService.RefreshTokenAsync(refreshToken, Response);

            if (tokenResponse == null)
            {
                return StatusCode(statusCode, new { Message = errorMessage });
            }

            return Ok(tokenResponse);
        }
    }
}