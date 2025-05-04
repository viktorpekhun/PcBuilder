using Microsoft.AspNetCore.Mvc;
using PcBuilderApi.Models;
using PcBuilderApi.Services.Implementations;
using static PcBuilderApi.Utilities.SD;

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
            await _scraperService.ScrapeCategoryAsync<Cpu>("https://hotline.ua/ua/computer/processory", ComponentType.Cpu);
            return Ok("Scraping completed for CPUs");
        }

        [HttpPost("gpu")]
        public async Task<IActionResult> ScrapeGpus()
        {
            //await _scraperService.ScrapeCategoryAsync<Gpu>("https://hotline.ua/ua/computer/videokarty/3991-36447-36449-36450-41408-43657-86245-380200-586473-643446-678073-21168069", ComponentType.Gpu);
            await _scraperService.ScrapeCategoryAsync<Gpu>("https://hotline.ua/ua/computer/videokarty/123146-294529-371577-609215/", ComponentType.Gpu);
            return Ok("Scraping completed for GPUs");
        }

        [HttpPost("motherboard")]
        public async Task<IActionResult> ScrapeMotherboards()
        {
            //await _scraperService.ScrapeCategoryAsync<Motherboard>("https://hotline.ua/ua/computer/materinskie-platy/448-449-4367-4987-40811-100226-574630-672507/", ComponentType.Motherboard);
            await _scraperService.ScrapeCategoryAsync<Motherboard>("https://hotline.ua/ua/computer/materinskie-platy/448-449-4367-4987-40811-100226-672507/", ComponentType.Motherboard);
            return Ok("Scraping completed for Motherboards");
        }

        [HttpPost("cpu-cooler")]
        public async Task<IActionResult> ScrapeCpuCoolers()
        {
            await _scraperService.ScrapeCategoryAsync<CpuCooler>("https://hotline.ua/ua/computer/kulery-i-radiatory/1570-3486-376753/", ComponentType.CpuCooler);
            //await _scraperService.ScrapeCategoryAsync<CpuCooler>("https://hotline.ua/ua/computer/kulery-i-radiatory/1570-3486-298637-376753/", ComponentType.CpuCooler);
            return Ok("Scraping completed for Cpu Coolers");
        }

        [HttpPost("pc-case")]
        public async Task<IActionResult> ScrapePcCases()
        {
            //await _scraperService.ScrapeCategoryAsync<PcCase>("https://hotline.ua/ua/computer/korpusa/65123-637240/", ComponentType.PcCase);
            await _scraperService.ScrapeCategoryAsync<PcCase>("https://hotline.ua/ua/computer/korpusa/", ComponentType.PcCase);
            return Ok("Scraping completed for PC Cases");
        }

        [HttpPost("power-supply")]
        public async Task<IActionResult> ScrapePowerSupplies()
        {
            //await _scraperService.ScrapeCategoryAsync<PowerSupply>("https://hotline.ua/ua/computer/bloki-pitaniya/2573-34733/", ComponentType.PowerSupply);
            await _scraperService.ScrapeCategoryAsync<PowerSupply>("https://hotline.ua/ua/computer/bloki-pitaniya/2573/", ComponentType.PowerSupply);
            return Ok("Scraping completed for Power Supplies");
        }

        [HttpPost("ram")]
        public async Task<IActionResult> ScrapeRams()
        {
            //await _scraperService.ScrapeCategoryAsync<Ram>("https://hotline.ua/ua/computer/moduli-pamyati-dlya-pk-i-noutbukov/3102-667301/", ComponentType.Ram);
            await _scraperService.ScrapeCategoryAsync<Ram>("https://hotline.ua/ua/computer/moduli-pamyati-dlya-pk-i-noutbukov/3102/", ComponentType.Ram);
            return Ok("Scraping completed for Rams");
        }

        [HttpPost("ssd")]
        public async Task<IActionResult> ScrapeSsds()
        {
            //await _scraperService.ScrapeCategoryAsync<Ssd>("https://hotline.ua/ua/computer/diski-ssd/9376-301672-389297-574411-619041-19934740/", ComponentType.Ssd);
            await _scraperService.ScrapeCategoryAsync<Ssd>("https://hotline.ua/ua/computer/diski-ssd/9376-389297-574411-619041-19934740/", ComponentType.Ssd);
            return Ok("Scraping completed for Ssds");
        }

        [HttpPost("hdd")]
        public async Task<IActionResult> ScrapeHdds()
        {
            //await _scraperService.ScrapeCategoryAsync<Hdd>("https://hotline.ua/ua/computer/diski-ssd/9376-301672-389297-574411-619041-19934740/", ComponentType.Hdd);
            await _scraperService.ScrapeCategoryAsync<Hdd>("https://hotline.ua/ua/computer/zhestkie-diski/9517/", ComponentType.Hdd);
            return Ok("Scraping completed for Hdds");
        }

        [HttpPost("fan")]
        public async Task<IActionResult> ScrapeFans()
        {
            //await _scraperService.ScrapeCategoryAsync<Fan>("https://hotline.ua/ua/computer/kulery-i-radiatory/1569-298705/", ComponentType.Fan);
            await _scraperService.ScrapeCategoryAsync<Fan>("https://hotline.ua/ua/computer/kulery-i-radiatory/1569/", ComponentType.Fan);
            return Ok("Scraping completed for Fans");
        }
    }
}
