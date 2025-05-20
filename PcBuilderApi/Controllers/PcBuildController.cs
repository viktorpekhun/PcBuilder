using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcBuilderApi.Dtos.PcBuildDtos;
using PcBuilderApi.Services.Interfaces;
using System.Security.Claims;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PcBuildController : ControllerBase
    {
        private readonly IPcBuildService _pcBuildService;

        public PcBuildController(IPcBuildService pcBuildService)
        {
            _pcBuildService = pcBuildService;
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckCompatibility([FromBody] ComponentsCompatibilityDto dto)
        {
            try
            {
                var results = await _pcBuildService.CheckComponentsCompatibilityAsync(dto);
                return Ok(new
                {
                    Compatible = !results.Any(r => !r.IsCompatible),
                    HasWarnings = results.Any(r => r.Messages.Any(m => m.Type == CompatibilityMessageType.Warning)),
                    Results = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("save")]
        public async Task<IActionResult> SaveBuild([FromBody] PcBuildInputDto buildDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return Unauthorized("Invalid user identification");
                }

                var result = await _pcBuildService.SaveBuildAsync(buildDto, userId);

                if (result)
                {
                    return Ok(new { Success = true, Message = "Build saved successfully" });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Failed to save build" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while saving the build",
                    Error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpGet("user-builds")]
        public async Task<IActionResult> GetUserBuilds()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return Unauthorized("Invalid user identification");
                }
                var builds = await _pcBuildService.GetUserBuildsAsync(userId);
                return Ok(builds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBuildById(Guid id)
        {
            try
            {
                Guid? userId = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid parsedUserId))
                {
                    userId = parsedUserId;
                }

                var build = await _pcBuildService.GetBuildByIdAsync(id);

                if (build == null)
                {
                    return NotFound(new { Success = false, Message = "Build not found" });
                }


                if (!build.IsPublished && (userId == null || build.UserId != userId.Value))
                {
                    return Forbid();
                }

                return Ok(build);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while retrieving the build",
                    Error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuild(Guid id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return Unauthorized("Invalid user identification");
                }
                var result = await _pcBuildService.DeleteBuildAsync(id, userId);
                if (result)
                {
                    return Ok(new { Success = true, Message = "Build deleted successfully" });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Failed to delete build" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while deleting the build",
                    Error = ex.Message
                });
            }

        }
    }
}
