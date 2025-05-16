using Microsoft.AspNetCore.Mvc;
using PcBuilderApi.Dtos.PcBuildDtos;
using PcBuilderApi.Services.Interfaces;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompatibilityController : ControllerBase
    {
        private readonly IPcBuildService _pcBuildService;

        public CompatibilityController(IPcBuildService pcBuildService)
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
    }
}
