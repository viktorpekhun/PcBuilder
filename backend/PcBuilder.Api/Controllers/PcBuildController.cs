using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Filtering;
using PcBuilds.Application.Commands;
using PcBuilds.Application.Dtos;
using PcBuilds.Application.Queries;

namespace PcBuilder.Api.Controllers
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
            var result = await _mediator.Send(new CheckCompatibilityQuery(dto));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            var results = result.Value!;
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
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true, Message = "Build saved successfully" });
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBuild(Guid id, [FromBody] PcBuildInputDto buildDto)
        {
            var result = await _mediator.Send(new UpdateBuildCommand(id, buildDto));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true, Message = "Build updated successfully" });
        }

        [Authorize]
        [HttpGet("user-builds")]
        public async Task<IActionResult> GetUserBuilds()
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new GetUserBuildsQuery(userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpGet("gallery")]
        public async Task<IActionResult> GetGallery([FromQuery] ResourceParameters parameters)
        {
            foreach (var key in Request.Query.Keys)
            {
                if (key.Equals("pageNumber", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("pageSize", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("orderBy", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("ascending", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("searchQuery", StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = Request.Query[key].ToString();

                if (value.StartsWith("[") && value.EndsWith("]"))
                {
                    try
                    {
                        var parsedValues = JsonSerializer.Deserialize<string[]>(value);
                        if (parsedValues != null && parsedValues.Length > 0)
                            parameters.Filters[key] = parsedValues;
                    }
                    catch
                    {
                        parameters.Filters[key] = new[] { value };
                    }
                }
                else
                {
                    parameters.Filters[key] = new[] { value };
                }
            }

            var result = await _mediator.Send(new GetPublicBuildsQuery(parameters));
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBuildById(Guid id)
        {
            var result = await _mediator.Send(new GetBuildByIdQuery(id));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            var build = result.Value!;

            if (!build.IsPublished)
            {
                var idStr = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(idStr, out var userId))
                    return Unauthorized(new { Message = "Authentication required to view private builds." });

                if (build.UserId != userId)
                    return StatusCode(403, new { Message = "You do not have access to this build." });
            }

            return Ok(build);
        }

        [Authorize]
        [HttpPut("{id:guid}/publish")]
        public async Task<IActionResult> PublishBuild(Guid id, [FromBody] PublishBuildRequest request)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new PublishPcBuildCommand(id, userId, request.IsPublished));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new
            {
                Success = true,
                Message = result.Value ? "Build published successfully" : "Build unpublished successfully"
            });
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBuild(Guid id)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new DeleteBuildCommand(id, userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true, Message = "Build deleted successfully" });
        }

        [HttpGet("{id:guid}/comments")]
        public async Task<IActionResult> GetBuildComments(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _mediator.Send(new GetBuildCommentsQuery(id, pageNumber, pageSize));
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

        [Authorize]
        [HttpPost("{id:guid}/comments")]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentDto dto)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new AddCommentCommand(id, userId, dto.Text, dto.Rating));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true, CommentId = result.Value });
        }

        [Authorize]
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new DeleteCommentCommand(commentId, userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true, Message = "Comment deleted" });
        }

        [Authorize]
        [HttpPost("{id:guid}/clone")]
        public async Task<IActionResult> CloneBuild(Guid id)
        {
            var userId = GetUserId();
            var result = await _mediator.Send(new CloneBuildCommand(id, userId));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(new { Success = true, BuildId = result.Value });
        }
    }

    public record PublishBuildRequest(bool IsPublished);
}