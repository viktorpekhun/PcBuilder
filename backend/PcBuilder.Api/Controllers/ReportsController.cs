using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moderation.Application.Commands;

namespace PcBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("review/{reviewId:guid}")]
        public async Task<IActionResult> ReportReview(Guid reviewId, [FromBody] ReportRequest request)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new ReportReviewCommand(userId, reviewId, request.Reason));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { ReportId = result.Value });
        }

        [HttpPost("build/{buildId:guid}")]
        public async Task<IActionResult> ReportBuild(Guid buildId, [FromBody] ReportRequest request)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new ReportBuildCommand(userId, buildId, request.Reason));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { ReportId = result.Value });
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

    public record ReportRequest(string Reason);
}