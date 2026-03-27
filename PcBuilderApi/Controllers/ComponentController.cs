using MediatR;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.SharedKernel.Filtering;
using PcBuilder.SharedKernel.Enums;
using Components.Application.Queries;
using System.Text.Json;

namespace PcBuilderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ComponentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{componentType}")]
        public async Task<IActionResult> GetAllComponentsByType(
            ComponentType componentType,
            [FromQuery] ResourceParameters parameters)
        {
            // Extract filter parameters from query
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
                        {
                            parameters.Filters[key] = parsedValues;
                        }
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

            var result = await _mediator.Send(new GetComponentsByTypeQuery(componentType, parameters));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            var pagedComponents = result.Value!;
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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

        [HttpGet("{componentType}/{id}")]
        public async Task<IActionResult> GetComponentById(ComponentType componentType, Guid id)
        {
            var result = await _mediator.Send(new GetComponentByIdQuery(id, componentType));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpGet("{componentType}/filter-options")]
        public async Task<IActionResult> GetFilterOptions(ComponentType componentType)
        {
            var filterOptions = await _mediator.Send(new GetFilterOptionsQuery(componentType));
            return Ok(filterOptions);
        }
    }
}
