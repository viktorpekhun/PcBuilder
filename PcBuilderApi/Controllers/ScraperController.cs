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

        [HttpPost("cpu")]
        public async Task<IActionResult> ScrapeCpus()
        {
            await _scraperService.TestScrapeCategoryAsync<Cpu>("https://hotline.ua/ua/computer/processory");
            return Ok("Scraping completed for CPUs");
        }

        [HttpPost("gpu")]
        public async Task<IActionResult> ScrapeGpus()
        {
            await _scraperService.TestScrapeCategoryAsync<Gpu>("https://hotline.ua/ua/computer/videokarty/3991-36447-36449-36450-41408-43657-86245-380200-586473-643446-678073-21168069");
            return Ok("Scraping completed for GPUs");
        }
    }
}
