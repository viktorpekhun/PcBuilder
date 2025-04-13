using Microsoft.AspNetCore.Mvc;
using PcBuilderApi.Models;
using PcBuilderApi.Services.Implementations;

namespace PcBuilderApi.Controllers
{
    [Route("api/scraper")]
    [ApiController]
    public class ScraperController : ControllerBase
    {
        private readonly ScraperService _scraperService;

        public ScraperController(ScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        [HttpPost("scrape/cpus")]
        public async Task<IActionResult> ScrapeCpus()
        {
            await _scraperService.TestScrapeCategoryAsync<Cpu>("https://hotline.ua/ua/computer/processory");
            return Ok("Scraping completed for CPUs");
        }
    }
}
