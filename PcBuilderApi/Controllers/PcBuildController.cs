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

        private Guid? UserId
        {
            get
            {
                var idStr = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(idStr, out var guid) ? guid : null;
            }
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

                if (UserId == null)
                {
                    return Unauthorized();
                }

                var result = await _pcBuildService.SaveBuildAsync(UserId.Value, buildDto);

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
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateBuild(Guid id, [FromBody] PcBuildInputDto buildDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (UserId == null)
                {
                    return Unauthorized();
                }
                var result = await _pcBuildService.UpdateBuildAsync(id, buildDto);
                if (result)
                {
                    return Ok(new { Success = true, Message = "Build updated successfully" });
                }
                else
                {
                    return BadRequest(new { Success = false, Message = "Failed to update build" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while updating the build",
                    Error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpGet("user-builds")]
        public async Task<ActionResult<IEnumerable<PcBuildListDto>>> GetUserBuilds()
        {
            try
            {
                if (UserId  == null)
                {
                    return Unauthorized();
                }
                var builds = await _pcBuildService.GetUserBuildsAsync(UserId.Value);
                return Ok(builds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<PcBuildRequestDto>> GetBuildById(Guid id)
        {
            try
            {
                var build = await _pcBuildService.GetBuildByIdAsync(id);

                if (build == null)
                {
                    return NotFound(new { Success = false, Message = "Build not found" });
                }

                if (!build.IsPublished && (UserId == null || build.UserId != UserId))
                {
                    return UserId == null ? Unauthorized() : Forbid();
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
                if (UserId == null)
                {
                    return Unauthorized();
                }
                var result = await _pcBuildService.DeleteBuildAsync(id, UserId.Value);
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
