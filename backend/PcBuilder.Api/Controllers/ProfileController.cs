using Auth.Application.Commands;
using Auth.Application.Dtos;
using Auth.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moderation.Application.Queries;
using PcBuilds.Application.Queries;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PcBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new GetProfileQuery(userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            var buildsResult = await _mediator.Send(new GetUserBuildsQuery(userId));
            result.Value!.BuildCount = buildsResult.IsSuccess ? buildsResult.Value!.Count : 0;

            return Ok(result.Value);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new UpdateProfileCommand(userId, request.Username, request.Bio));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            var buildsResult = await _mediator.Send(new GetUserBuildsQuery(userId));
            result.Value!.BuildCount = buildsResult.IsSuccess ? buildsResult.Value!.Count : 0;

            return Ok(result.Value);
        }

        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file provided." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { Message = "File size cannot exceed 5 MB." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { Message = "Only JPEG, PNG, and WebP images are allowed." });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var result = await _mediator.Send(new UploadAvatarCommand(GetUserId(), ms.ToArray()));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { avatarUrl = result.Value });
        }

        [HttpDelete("avatar")]
        public async Task<IActionResult> DeleteAvatar()
        {
            var result = await _mediator.Send(new DeleteAvatarCommand(GetUserId()));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Message = result.Value });
        }

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto request)
        {
            var result = await _mediator.Send(new SetPasswordCommand(GetUserId(), request.NewPassword));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Message = result.Value });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var result = await _mediator.Send(new ChangePasswordCommand(GetUserId(), request.CurrentPassword, request.NewPassword));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Message = result.Value });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var result = await _mediator.Send(new DeleteAccountCommand(GetUserId()));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.Now.AddDays(-1)
            });

            return Ok(new { Message = result.Value });
        }

        [HttpGet("bans")]
        public async Task<IActionResult> GetBans()
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new GetUserBansQuery(userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(result.Value);
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
    }
}
