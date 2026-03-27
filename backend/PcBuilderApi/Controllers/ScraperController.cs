using Components.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application.Commands;

namespace PcBuilder.Api.Controllers
{
    [Route("api/scraper")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("scraper")]
    public class ScraperController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ScraperController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("cpu")]
        public async Task<IActionResult> ScrapeCpus()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/processory", ComponentType.Cpu, typeof(Cpu)));
            return Ok("Scraping completed for CPUs");
        }

        [HttpPost("gpu")]
        public async Task<IActionResult> ScrapeGpus()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/videokarty/3991-36447-36449-36450-41408-43657-86245-380200-586473-643446-678073-21168069", ComponentType.Gpu, typeof(Gpu)));
            await _mediator.Send(new CorrectGpuModelsCommand());
            return Ok("Scraping completed for GPUs");
        }

        [HttpPost("correct_gpu")]
        public async Task<IActionResult> CorrectGpu()
        {
            await _mediator.Send(new CorrectGpuModelsCommand());
            return Ok("GPU models corrected");
        }

        [HttpPost("motherboard")]
        public async Task<IActionResult> ScrapeMotherboards()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/materinskie-platy/448-449-4367-4987-40811-100226-672507/", ComponentType.Motherboard, typeof(Motherboard)));
            return Ok("Scraping completed for Motherboards");
        }

        [HttpPost("cpu-cooler")]
        public async Task<IActionResult> ScrapeCpuCoolers()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/kulery-i-radiatory/1570-3486-376753/", ComponentType.CpuCooler, typeof(CpuCooler)));
            return Ok("Scraping completed for Cpu Coolers");
        }

        [HttpPost("pc-case")]
        public async Task<IActionResult> ScrapePcCases()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/korpusa/", ComponentType.PcCase, typeof(PcCase)));
            return Ok("Scraping completed for PC Cases");
        }

        [HttpPost("power-supply")]
        public async Task<IActionResult> ScrapePowerSupplies()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/bloki-pitaniya/2573/", ComponentType.PowerSupply, typeof(PowerSupply)));
            return Ok("Scraping completed for Power Supplies");
        }

        [HttpPost("ram")]
        public async Task<IActionResult> ScrapeRams()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/moduli-pamyati-dlya-pk-i-noutbukov/3102/", ComponentType.Ram, typeof(Ram)));
            return Ok("Scraping completed for Rams");
        }

        [HttpPost("ssd")]
        public async Task<IActionResult> ScrapeSsds()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/diski-ssd/9376-389297-574411-619041-19934740/", ComponentType.Ssd, typeof(Ssd)));
            return Ok("Scraping completed for Ssds");
        }

        [HttpPost("hdd")]
        public async Task<IActionResult> ScrapeHdds()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/zhestkie-diski/9517/", ComponentType.Hdd, typeof(Hdd)));
            return Ok("Scraping completed for Hdds");
        }

        [HttpPost("fan")]
        public async Task<IActionResult> ScrapeFans()
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/kulery-i-radiatory/1569/", ComponentType.Fan, typeof(Fan)));
            return Ok("Scraping completed for Fans");
        }
    }
}
