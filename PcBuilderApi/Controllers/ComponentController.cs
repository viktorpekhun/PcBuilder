using Microsoft.AspNetCore.Mvc;
using PcBuilderApi.Services.Interfaces;
using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentController : ControllerBase
    {
        private readonly IComponentService _componentService;

        public ComponentController(IComponentService componentService)
        {
            _componentService = componentService;
        }

        [HttpGet("{componentType}")]
        public async Task<IActionResult> GetAllComponentsByType(ComponentType componentType)
        {
            try
            {
                var components = await _componentService.GetAllByTypeAsync(componentType);
                return Ok(components);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

        [HttpGet("{componentType}/{id}")]
        public async Task<IActionResult> GetComponentById(ComponentType componentType, Guid id)
        {
            try
            {
                var component = await _componentService.GetByIdAsync(id, componentType);
                return Ok(component);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }

    }
}
