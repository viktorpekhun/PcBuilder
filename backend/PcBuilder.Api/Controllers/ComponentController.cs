using Components.Application.PreFilter;
using Components.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Filtering;
using System.Text.Json;

namespace PcBuilder.Api.Controllers
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

        private static readonly HashSet<string> _reservedKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "pageNumber", "pageSize", "orderBy", "ascending", "searchQuery",
            "prefilter", "pb_cpu", "pb_gpu", "pb_mb", "pb_ram", "pb_cooler", "pb_case", "pb_psu"
        };

        [HttpGet("{componentType}")]
        public async Task<IActionResult> GetAllComponentsByType(
            ComponentType componentType,
            [FromQuery] ResourceParameters parameters)
        {
            // Extract filter parameters from query (skip pagination and prefilter keys)
            foreach (var key in Request.Query.Keys)
            {
                if (_reservedKeys.Contains(key))
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

            // Prefilter params
            bool applyPrefilter = Request.Query.TryGetValue("prefilter", out var pfVal) &&
                                   pfVal.ToString().Equals("true", StringComparison.OrdinalIgnoreCase);

            PartialBuildIds? partialBuildIds = null;
            if (applyPrefilter)
            {
                partialBuildIds = new PartialBuildIds(
                    CpuId:          TryParseGuid(Request.Query["pb_cpu"]),
                    GpuId:          TryParseGuid(Request.Query["pb_gpu"]),
                    MotherboardId:  TryParseGuid(Request.Query["pb_mb"]),
                    RamId:          TryParseGuid(Request.Query["pb_ram"]),
                    CpuCoolerId:    TryParseGuid(Request.Query["pb_cooler"]),
                    PcCaseId:       TryParseGuid(Request.Query["pb_case"]),
                    PowerSupplyId:  TryParseGuid(Request.Query["pb_psu"]));
            }

            var result = await _mediator.Send(new GetComponentsByTypeQuery(componentType, parameters, applyPrefilter, partialBuildIds));
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

        private static Guid? TryParseGuid(Microsoft.Extensions.Primitives.StringValues value)
        {
            var str = value.ToString();
            return Guid.TryParse(str, out var guid) ? guid : null;
        }

        [HttpGet("{componentType}/{id:guid}/price-history")]
        public async Task<IActionResult> GetPriceHistory(
            ComponentType componentType,
            Guid id,
            [FromQuery] int days = 30)
        {
            var result = await _mediator.Send(new GetPriceHistoryQuery(componentType, id, days));
            if (result.IsFailure)
                return StatusCode(result.Error!.StatusCode, new { Message = result.Error.Message });

            return Ok(result.Value);
        }
    }
}
