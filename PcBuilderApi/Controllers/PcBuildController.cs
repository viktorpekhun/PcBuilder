using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Exceptions;
using PcBuilds.Application.Commands;
using PcBuilds.Application.Dtos;
using PcBuilds.Application.Queries;

namespace PcBuilderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PcBuildController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PcBuildController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetUserId()
        {
            var idStr = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                        ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(idStr, out var guid))
                throw new UnauthorizedAccessException("User ID not found in token.");

            return guid;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckCompatibility([FromBody] ComponentsCompatibilityDto dto)
        {
            var results = await _mediator.Send(new CheckCompatibilityQuery(dto));
            return Ok(new
            {
                Compatible = !results.Any(r => !r.IsCompatible),
                HasWarnings = results.Any(r => r.Messages.Any(m => m.Type == CompatibilityMessageType.Warning)),
                Results = results
            });
        }

        [Authorize]
        [HttpPost("save")]
        public async Task<IActionResult> SaveBuild([FromBody] PcBuildInputDto buildDto)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new SaveBuildCommand(userId, buildDto));

            if (!result)
                return BadRequest(new { Success = false, Message = "Failed to save build" });

            return Ok(new { Success = true, Message = "Build saved successfully" });
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBuild(Guid id, [FromBody] PcBuildInputDto buildDto)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new UpdateBuildCommand(id, buildDto));

            if (!result)
                return BadRequest(new { Success = false, Message = "Failed to update build" });

            return Ok(new { Success = true, Message = "Build updated successfully" });
        }

        [Authorize]
        [HttpGet("user-builds")]
        public async Task<IActionResult> GetUserBuilds()
        {
            var userId = GetUserId();
            var builds = await _mediator.Send(new GetUserBuildsQuery(userId));
            return Ok(builds);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBuildById(Guid id)
        {
            var build = await _mediator.Send(new GetBuildByIdQuery(id));

            if (build == null)
                throw new NotFoundException("Build not found");

            if (!build.IsPublished)
            {
                var idStr = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(idStr, out var userId))
                    throw new UnauthorizedAccessException("Authentication required to view private builds.");

                if (build.UserId != userId)
                    throw new ForbiddenException("You do not have access to this build.");
            }

            return Ok(build);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuild(Guid id)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new DeleteBuildCommand(id, userId));

            if (!result)
                return BadRequest(new { Success = false, Message = "Failed to delete build" });

            return Ok(new { Success = true, Message = "Build deleted successfully" });
        }
    }
}
