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
    //[Authorize]
    //[EnableRateLimiting("scraper")]
    public class ScraperController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ScraperController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //[HttpPost("single-gpu")]
        //public async Task<IActionResult> ScrapeGpu(CancellationToken cancellationToken)
        //{
        //    await _mediator.Send(new ScrapeSingleComponentCommand("https://hotline.ua/ua/computer-videokarty/asus-prime-rtx5070-o12g", ComponentType.Gpu, typeof(Gpu)), cancellationToken);
        //    return Ok("Scraping completed for GPU");
        //}
        [HttpPost("single-powersupply")]
        public async Task<IActionResult> ScrapePS(
                [FromBody] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required");
            }

            await _mediator.Send(new ScrapeSingleComponentCommand(
                url,
                ComponentType.PowerSupply,
                typeof(PowerSupply), new[] { typeof(PowerSupplyPowerConnector) }),
                cancellationToken);

            return Ok("Scraping completed for powersupply");
        }
        [HttpPost("single-cpucooler")]
        public async Task<IActionResult> ScrapeCpuCooler(
                [FromBody] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required");
            }

            await _mediator.Send(new ScrapeSingleComponentCommand(
                url,
                ComponentType.CpuCooler,
                typeof(CpuCooler), new[] { typeof(CpuCoolerSocket) }),
                cancellationToken);

            return Ok("Scraping completed for CpuCooler");
        }

        [HttpPost("single-pccase")]
        public async Task<IActionResult> ScrapePcCase(
                [FromBody] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required");
            }

            await _mediator.Send(new ScrapeSingleComponentCommand(
                url,
                ComponentType.PcCase,
                typeof(PcCase), new[] { typeof(PcCaseFormFactor), typeof(PcCaseFanLocation) }),
                cancellationToken);

            return Ok("Scraping completed for PcCase");
        }

        [HttpPost("single-ram")]
        public async Task<IActionResult> ScrapeRam(
                [FromBody] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required");
            }

            await _mediator.Send(new ScrapeSingleComponentCommand(
                url,
                ComponentType.Ram,
                typeof(Ram)),
                cancellationToken);

            return Ok("Scraping completed for Ram");
        }

        [HttpPost("cpu")]
        public async Task<IActionResult> ScrapeCpus(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/processory", ComponentType.Cpu, typeof(Cpu)), cancellationToken);
            return Ok("Scraping completed for CPUs");
        }

        [HttpPost("gpu")]
        public async Task<IActionResult> ScrapeGpus(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/videokarty/3991-36447-36449-36450-41408-43657-86245-380200-586473-643446-678073-21168069", ComponentType.Gpu, typeof(Gpu)), cancellationToken);
            await _mediator.Send(new CorrectGpuModelsCommand(), cancellationToken);
            return Ok("Scraping completed for GPUs");
        }

        [HttpPost("motherboard")]
        public async Task<IActionResult> ScrapeMotherboards(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/materinskie-platy/448-449-4367-4987-40811-100226-672507/", ComponentType.Motherboard, typeof(Motherboard)), cancellationToken);
            return Ok("Scraping completed for Motherboards");
        }

        [HttpPost("cpu-cooler")]
        public async Task<IActionResult> ScrapeCpuCoolers(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/kulery-i-radiatory/1570-3486-376753/", ComponentType.CpuCooler, typeof(CpuCooler)), cancellationToken);
            return Ok("Scraping completed for Cpu Coolers");
        }

        [HttpPost("pc-case")]
        public async Task<IActionResult> ScrapePcCases(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/korpusa/", ComponentType.PcCase, typeof(PcCase)), cancellationToken);
            return Ok("Scraping completed for PC Cases");
        }

        [HttpPost("power-supply")]
        public async Task<IActionResult> ScrapePowerSupplies(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/bloki-pitaniya/2573/", ComponentType.PowerSupply, typeof(PowerSupply)), cancellationToken);
            return Ok("Scraping completed for Power Supplies");
        }

        [HttpPost("ram")]
        public async Task<IActionResult> ScrapeRams(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/moduli-pamyati-dlya-pk-i-noutbukov/3102/", ComponentType.Ram, typeof(Ram)), cancellationToken);
            return Ok("Scraping completed for Rams");
        }

        [HttpPost("ssd")]
        public async Task<IActionResult> ScrapeSsds(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/diski-ssd/9376-389297-574411-619041-19934740/", ComponentType.Ssd, typeof(Ssd)), cancellationToken);
            return Ok("Scraping completed for Ssds");
        }

        [HttpPost("hdd")]
        public async Task<IActionResult> ScrapeHdds(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/zhestkie-diski/9517/", ComponentType.Hdd, typeof(Hdd)), cancellationToken);
            return Ok("Scraping completed for Hdds");
        }

        [HttpPost("fan")]
        public async Task<IActionResult> ScrapeFans(CancellationToken cancellationToken)
        {
            await _mediator.Send(new ScrapeCategoryCommand("https://hotline.ua/ua/computer/kulery-i-radiatory/1569/", ComponentType.Fan, typeof(Fan)), cancellationToken);
            return Ok("Scraping completed for Fans");
        }
    }
}
