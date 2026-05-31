using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moderation.Application.Commands;
using Moderation.Application.Queries;
using Moderation.Application.Dtos;
using Moderation.Domain.Enums;

namespace PcBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- Reports ---

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports(
            [FromQuery] ReportStatus? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetReportsQuery(status, pageNumber, pageSize));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var pagedResult = result.Value!;

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new
            {
                pagedResult.TotalCount,
                pagedResult.PageSize,
                pagedResult.PageNumber,
                pagedResult.TotalPages,
                pagedResult.HasNext,
                pagedResult.HasPrevious
            }, jsonOptions));

            return Ok(pagedResult.Items);
        }

        [HttpPost("reports/{reportId:guid}/resolve")]
        public async Task<IActionResult> ResolveReport(Guid reportId, [FromBody] ResolveReportRequest request)
        {
            var adminId = GetUserId();
            var result = await _mediator.Send(new ResolveReportCommand(
                adminId,
                reportId,
                request.Action,
                request.Reason,
                request.BanType,
                request.BanDurationDays));

            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true });
        }

        // --- Users ---

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? searchQuery = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetAdminUsersQuery(searchQuery, pageNumber, pageSize));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var pagedResult = result.Value!;

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new
            {
                pagedResult.TotalCount,
                pagedResult.PageSize,
                pagedResult.PageNumber,
                pagedResult.TotalPages,
                pagedResult.HasNext,
                pagedResult.HasPrevious
            }, jsonOptions));

            return Ok(pagedResult.Items);
        }

        [HttpGet("users/{userId:guid}")]
        public async Task<IActionResult> GetUserDetail(Guid userId)
        {
            var result = await _mediator.Send(new GetAdminUserDetailQuery(userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpPatch("users/{userId:guid}/role")]
        public async Task<IActionResult> ChangeUserRole(Guid userId, [FromBody] ChangeRoleRequest request)
        {
            var adminId = GetUserId();
            var result = await _mediator.Send(new ChangeUserRoleCommand(adminId, userId, request.Role));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true, Promoted = result.Value });
        }

        [HttpPost("users/{userId:guid}/ban")]
        public async Task<IActionResult> BanUser(Guid userId, [FromBody] BanUserRequest request)
        {
            var adminId = GetUserId();
            var result = await _mediator.Send(new IssueBanCommand(adminId, userId, request.BanType, request.DurationDays, request.Reason));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true });
        }

        [HttpPost("users/{userId:guid}/unban")]
        public async Task<IActionResult> UnbanUser(Guid userId, [FromBody] UnbanUserRequest request)
        {
            var adminId = GetUserId();
            var result = await _mediator.Send(new RevokeBanCommand(adminId, userId, request.BanType));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true });
        }

        [HttpDelete("users/{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var adminId = GetUserId();
            var result = await _mediator.Send(new AdminDeleteUserCommand(adminId, userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true });
        }

        // --- Content ---

        [HttpDelete("content/reviews/{reviewId:guid}")]
        public async Task<IActionResult> DeleteReview(Guid reviewId)
        {
            var adminId = GetUserId();
            var result = await _mediator.Send(new AdminDeleteReviewCommand(adminId, reviewId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true });
        }

        [HttpDelete("content/builds/{buildId:guid}")]
        public async Task<IActionResult> DeleteBuild(Guid buildId)
        {
            var adminId = GetUserId();
            var result = await _mediator.Send(new AdminDeleteBuildCommand(adminId, buildId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true });
        }

        // --- Activity ---

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity(
            [FromQuery] int daysBack = 1,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetAdminActivityQuery(daysBack, pageNumber, pageSize));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var pagedResult = result.Value!;

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new
            {
                pagedResult.TotalCount,
                pagedResult.PageSize,
                pagedResult.PageNumber,
                pagedResult.TotalPages,
                pagedResult.HasNext,
                pagedResult.HasPrevious
            }, jsonOptions));

            return Ok(pagedResult.Items);
        }

        // --- Stats ---

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var result = await _mediator.Send(new GetAdminStatsQuery());
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

    public record ResolveReportRequest(
        ReportResolutionAction Action,
        string? Reason = null,
        BanType? BanType = null,
        int? BanDurationDays = null);

    public record ChangeRoleRequest(string Role);

    public record BanUserRequest(BanType BanType, int DurationDays, string Reason);

    public record UnbanUserRequest(BanType BanType);
}