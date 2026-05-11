using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Components.Application.Commands;
using Components.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.SharedKernel.Enums;

namespace PcBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PriceAlertsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriceAlertsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public record CreatePriceAlertRequest(Guid ComponentId, ComponentType ComponentType, decimal ThresholdPercent);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePriceAlertRequest request)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new CreatePriceAlertCommand(
                userId,
                request.ComponentId,
                request.ComponentType,
                request.ThresholdPercent));

            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Id = result.Value });
        }

        [HttpGet("for-component")]
        public async Task<IActionResult> GetForComponent(
            [FromQuery] Guid componentId,
            [FromQuery] ComponentType componentType)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new GetPriceAlertForComponentQuery(userId, componentId, componentType));

            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            if (result.Value is null)
                return NoContent();

            return Ok(result.Value);
        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetMine()
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new GetUserPriceAlertsQuery(userId));

            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new DeletePriceAlertCommand(userId, id));

            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true });
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
