using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.Application.Commands;
using Notifications.Application.Queries;

namespace PcBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? onlyUnread = null)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new GetNotificationsQuery(userId, onlyUnread, pageNumber, pageSize));
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

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new GetUnreadCountQuery(userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { count = result.Value });
        }

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new MarkNotificationReadCommand(userId, id));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true });
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new MarkAllReadCommand(userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true, MarkedCount = result.Value });
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
