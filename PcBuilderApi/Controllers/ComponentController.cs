using Microsoft.AspNetCore.Mvc;
using PcBuilderApi.Services.Interfaces;
using PcBuilderApi.Utilities.Filtering;
using System.Text.Json;
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
        public async Task<IActionResult> GetAllComponentsByType(
            ComponentType componentType,
            [FromQuery] ResourceParameters parameters)
        {
            try
            {
                // Extract filter parameters from query
                foreach (var key in Request.Query.Keys)
                {
                    // Skip standard parameter names and null values
                    if (key.Equals("pageNumber", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("pageSize", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("orderBy", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("ascending", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("searchQuery", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var value = Request.Query[key].ToString();

                    // Check if the value is a JSON array string
                    if (value.StartsWith("[") && value.EndsWith("]"))
                    {
                        try
                        {
                            // Deserialize the JSON array string to string array
                            var parsedValues = JsonSerializer.Deserialize<string[]>(value);
                            if (parsedValues != null && parsedValues.Length > 0)
                            {
                                parameters.Filters[key] = parsedValues;
                            }
                        }
                        catch
                        {
                            // Fallback to treating as single value if parsing fails
                            parameters.Filters[key] = new[] { value };
                        }
                    }
                    else
                    {
                        // Regular single value
                        parameters.Filters[key] = new[] { value };
                    }
                }

                var pagedComponents = await _componentService.GetAllByTypeAsync(componentType, parameters);
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                // Add pagination headers
                Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new
                {
                    pagedComponents.TotalCount,
                    pagedComponents.PageSize,
                    pagedComponents.PageNumber,
                    pagedComponents.TotalPages,
                    pagedComponents.HasNext,
                    pagedComponents.HasPrevious
                }, jsonOptions));

                return Ok(pagedComponents.Items);
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

        [HttpGet("{componentType}/filter-options")]
        public async Task<IActionResult> GetFilterOptions(ComponentType componentType)
        {
            try
            {
                var filterOptions = await _componentService.GetFilterOptionsAsync(componentType);

                return Ok(filterOptions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving filter options." });
            }
        }
    }
}
