using MediatR;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.Api.Search;

namespace PcBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Ok(new List<GlobalSearchItemDto>());

            var result = await _mediator.Send(new GlobalSearchQuery(q, limit));
            return Ok(result);
        }
    }
}
