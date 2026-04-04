using Components.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.Api.Models;
using PcBuilder.Api.Services;
using PcBuilder.Contracts.Messages;

namespace PcBuilder.Api.Controllers
{
    [Route("api/scraper")]
    [ApiController]
    //[Authorize]
    //[EnableRateLimiting("scraper")]
    public class ScraperController : ControllerBase
    {
        private readonly IRabbitMqPublisher _publisher;
        private readonly IScrapeJobTracker _tracker;

        public ScraperController(IRabbitMqPublisher publisher, IScrapeJobTracker tracker)
        {
            _publisher = publisher;
            _tracker = tracker;
        }

        [HttpGet("jobs")]
        public IActionResult GetAllJobs()
        {
            return Ok(_tracker.GetAllStatuses());
        }

        [HttpGet("jobs/{jobId:guid}")]
        public IActionResult GetJobStatus(Guid jobId)
        {
            var status = _tracker.GetStatus(jobId);
            if (status == null) return NotFound();
            return Ok(status);
        }

        [HttpPost("single-powersupply")]
        public async Task<IActionResult> ScrapePS([FromBody] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL is required");

            return await EnqueueSingleAsync(url, "PowerSupply", nameof(PowerSupply), cancellationToken);
        }

        [HttpPost("single-cpucooler")]
        public async Task<IActionResult> ScrapeCpuCooler([FromBody] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL is required");

            return await EnqueueSingleAsync(url, "CpuCooler", nameof(CpuCooler), cancellationToken);
        }

        [HttpPost("single-pccase")]
        public async Task<IActionResult> ScrapePcCase([FromBody] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL is required");

            return await EnqueueSingleAsync(url, "PcCase", nameof(PcCase), cancellationToken);
        }

        [HttpPost("single-ram")]
        public async Task<IActionResult> ScrapeRam([FromBody] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("URL is required");

            return await EnqueueSingleAsync(url, "Ram", nameof(Ram), cancellationToken);
        }

        [HttpPost("cpu")]
        public async Task<IActionResult> ScrapeCpus(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/processory", "Cpu", nameof(Cpu), cancellationToken);

        [HttpPost("gpu")]
        public async Task<IActionResult> ScrapeGpus(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync(
                "https://hotline.ua/ua/computer/videokarty/3991-36447-36449-36450-41408-43657-86245-380200-586473-643446-678073-21168069",
                "Gpu", nameof(Gpu), cancellationToken, correctGpuModels: true);

        [HttpPost("motherboard")]
        public async Task<IActionResult> ScrapeMotherboards(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/materinskie-platy/448-449-4367-4987-40811-100226-672507/", "Motherboard", nameof(Motherboard), cancellationToken);

        [HttpPost("cpu-cooler")]
        public async Task<IActionResult> ScrapeCpuCoolers(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/kulery-i-radiatory/1570-3486-376753/", "CpuCooler", nameof(CpuCooler), cancellationToken);

        [HttpPost("pc-case")]
        public async Task<IActionResult> ScrapePcCases(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/korpusa/", "PcCase", nameof(PcCase), cancellationToken);

        [HttpPost("power-supply")]
        public async Task<IActionResult> ScrapePowerSupplies(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/bloki-pitaniya/2573/", "PowerSupply", nameof(PowerSupply), cancellationToken);

        [HttpPost("ram")]
        public async Task<IActionResult> ScrapeRams(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/moduli-pamyati-dlya-pk-i-noutbukov/3102/", "Ram", nameof(Ram), cancellationToken);

        [HttpPost("ssd")]
        public async Task<IActionResult> ScrapeSsds(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/diski-ssd/9376-389297-574411-619041-19934740/", "Ssd", nameof(Ssd), cancellationToken);

        [HttpPost("hdd")]
        public async Task<IActionResult> ScrapeHdds(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/zhestkie-diski/9517/", "Hdd", nameof(Hdd), cancellationToken);

        [HttpPost("fan")]
        public async Task<IActionResult> ScrapeFans(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/kulery-i-radiatory/1569/", "Fan", nameof(Fan), cancellationToken);

        private async Task<IActionResult> EnqueueCategoryAsync(string url, string componentType, string entityTypeName, CancellationToken ct, bool correctGpuModels = false)
        {
            if (_tracker.HasActiveJob(componentType))
                return Conflict($"A scrape job for {componentType} is already running or queued.");

            var jobId = Guid.NewGuid();
            var message = new ScrapeJobMessage(jobId, url, componentType, entityTypeName, "Category", null, correctGpuModels);

            _tracker.TrackJob(new ScrapeJobStatus
            {
                JobId = jobId,
                ComponentType = componentType,
                State = "Queued",
                QueuedAt = DateTime.UtcNow
            });

            await _publisher.PublishAsync("scrape-jobs", message, ct);

            return Accepted($"/api/scraper/jobs/{jobId}", new { jobId, status = "Queued", componentType });
        }

        private async Task<IActionResult> EnqueueSingleAsync(string url, string componentType, string entityTypeName, CancellationToken ct)
        {
            var jobId = Guid.NewGuid();
            var message = new ScrapeJobMessage(jobId, url, componentType, entityTypeName, "SingleComponent", null, false);

            _tracker.TrackJob(new ScrapeJobStatus
            {
                JobId = jobId,
                ComponentType = componentType,
                State = "Queued",
                QueuedAt = DateTime.UtcNow
            });

            await _publisher.PublishAsync("scrape-jobs", message, ct);

            return Accepted($"/api/scraper/jobs/{jobId}", new { jobId, status = "Queued", componentType });
        }
    }
}
