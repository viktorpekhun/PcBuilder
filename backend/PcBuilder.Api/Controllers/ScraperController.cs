using Components.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PcBuilder.Api.Models;
using PcBuilder.Api.Services;
using PcBuilder.Contracts.Messages;
using Scraping.Application.Commands;
using Scraping.Infrastructure.Handlers;
using Scraping.Infrastructure.Scrapers.PassMark;

namespace PcBuilder.Api.Controllers
{
    [Route("api/scraper")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class ScraperController : ControllerBase
    {
        private readonly IRabbitMqPublisher _publisher;
        private readonly IScrapeJobTracker _tracker;
        private readonly ISender _sender;
        private readonly PassMarkUpdateJobHandler _passMarkHandler;

        public ScraperController(IRabbitMqPublisher publisher, IScrapeJobTracker tracker, ISender sender, PassMarkUpdateJobHandler passMarkHandler)
        {
            _publisher = publisher;
            _tracker = tracker;
            _sender = sender;
            _passMarkHandler = passMarkHandler;
        }

        [HttpGet("jobs")]
        public IActionResult GetAllJobs()
        {
            return Ok(_tracker.GetAllStatuses());
        }

        [HttpGet("jobs/latest")]
        public IActionResult GetLatestJobPerType()
        {
            var latest = _tracker.GetAllStatuses()
                .GroupBy(j => j.ComponentType)
                .Select(g => g.OrderByDescending(j => j.QueuedAt).First())
                .ToList();
            return Ok(latest);
        }

        [HttpGet("jobs/{jobId:guid}")]
        public IActionResult GetJobStatus(Guid jobId)
        {
            var status = _tracker.GetStatus(jobId);
            if (status == null) return NotFound();
            return Ok(status);
        }

        [HttpDelete("jobs/{jobId:guid}")]
        public async Task<IActionResult> CancelJob(Guid jobId, CancellationToken cancellationToken)
        {
            var status = _tracker.GetStatus(jobId);
            if (status == null) return NotFound();

            if (status.State is "Completed" or "Failed" or "Cancelled")
                return Conflict($"Job is already {status.State}.");

            _tracker.MarkCancelled(jobId);
            await _publisher.PublishAsync("scrape-cancellations", new ScrapeJobCancelMessage(jobId), cancellationToken);

            return Ok(new { jobId, status = "Cancelling" });
        }

        [HttpPost("correct/gpu-models")]
        public async Task<IActionResult> CorrectGpuModels(CancellationToken cancellationToken)
        {
            await _sender.Send(new CorrectGpuModelsCommand(), cancellationToken);
            return Ok();
        }

        [HttpPost("translate/pc-case")]
        public async Task<IActionResult> TranslatePcCase(CancellationToken cancellationToken)
        {
            await _sender.Send(new TranslatePcCaseFieldsCommand(), cancellationToken);
            return Ok();
        }

        [HttpPost("translate/fan")]
        public async Task<IActionResult> TranslateFan(CancellationToken cancellationToken)
        {
            await _sender.Send(new TranslateFanFieldsCommand(), cancellationToken);
            return Ok();
        }

        [HttpPost("translate/power-supply")]
        public async Task<IActionResult> TranslatePowerSupply(CancellationToken cancellationToken)
        {
            await _sender.Send(new TranslatePowerSupplyFieldsCommand(), cancellationToken);
            return Ok();
        }

        [HttpPost("translate/cpu-cooler")]
        public async Task<IActionResult> TranslateCpuCooler(CancellationToken cancellationToken)
        {
            await _sender.Send(new TranslateCpuCoolerFieldsCommand(), cancellationToken);
            return Ok();
        }

        [HttpPost("translate/ram")]
        public async Task<IActionResult> TranslateRam(CancellationToken cancellationToken)
        {
            await _sender.Send(new TranslateRamFieldsCommand(), cancellationToken);
            return Ok();
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
        {
            var result = await EnqueueCategoryAsync("https://hotline.ua/ua/computer/kulery-i-radiatory/1570-3486-376753/", "CpuCooler", nameof(CpuCooler), cancellationToken);
            await _sender.Send(new TranslateCpuCoolerFieldsCommand(), cancellationToken);
            return Ok(result);
        }
        [HttpPost("pc-case")]
        public async Task<IActionResult> ScrapePcCases(CancellationToken cancellationToken)
        {
            var result = await EnqueueCategoryAsync("https://hotline.ua/ua/computer/korpusa/", "PcCase", nameof(PcCase), cancellationToken);
            await _sender.Send(new TranslatePcCaseFieldsCommand(), cancellationToken);
            return Ok(result);
        }

        [HttpPost("power-supply")]
        public async Task<IActionResult> ScrapePowerSupplies(CancellationToken cancellationToken)
        {
            var result = await EnqueueCategoryAsync("https://hotline.ua/ua/computer/bloki-pitaniya/2573/", "PowerSupply", nameof(PowerSupply), cancellationToken);
            await _sender.Send(new TranslatePowerSupplyFieldsCommand(), cancellationToken);
            return Ok(result);
        }
           
        [HttpPost("ram")]
        public async Task<IActionResult> ScrapeRams(CancellationToken cancellationToken)
        {
            var result = await EnqueueCategoryAsync("https://hotline.ua/ua/computer/moduli-pamyati-dlya-pk-i-noutbukov/3102/", "Ram", nameof(Ram), cancellationToken);
            await _sender.Send(new TranslateRamFieldsCommand(), cancellationToken);
            return Ok(result);
        }

        [HttpPost("ssd")]
        public async Task<IActionResult> ScrapeSsds(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/diski-ssd/9376-389297-574411-619041-19934740/", "Ssd", nameof(Ssd), cancellationToken);

        [HttpPost("hdd")]
        public async Task<IActionResult> ScrapeHdds(CancellationToken cancellationToken)
            => await EnqueueCategoryAsync("https://hotline.ua/ua/computer/zhestkie-diski/9517/", "Hdd", nameof(Hdd), cancellationToken);

        [HttpPost("fan")]
        public async Task<IActionResult> ScrapeFans(CancellationToken cancellationToken)
        {
            var result = await EnqueueCategoryAsync("https://hotline.ua/ua/computer/kulery-i-radiatory/1569/", "Fan", nameof(Fan), cancellationToken);
            await _sender.Send(new TranslateFanFieldsCommand(), cancellationToken);
            return Ok(result);
        }

        [HttpPost("passmark/preview")]
        public async Task<IActionResult> PreviewPassMark(CancellationToken cancellationToken)
        {
            var result = await _passMarkHandler.PreviewAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPost("passmark")]
        public async Task<IActionResult> UpdatePassMark(CancellationToken cancellationToken)
        {
            const string componentType = "PassMark";

            if (_tracker.HasActiveJob(componentType))
                return Conflict("A PassMark update job is already running or queued.");

            var jobId = Guid.NewGuid();
            var message = new ScrapeJobMessage(jobId, string.Empty, componentType, string.Empty, "PassMarkUpdate", null, false);

            _tracker.TrackJob(new ScrapeJobStatus
            {
                JobId = jobId,
                ComponentType = componentType,
                Kind = "PassMarkUpdate",
                State = "Queued",
                QueuedAt = DateTime.UtcNow
            });

            await _publisher.PublishAsync("scrape-jobs", message, cancellationToken);

            return Accepted($"/api/scraper/jobs/{jobId}", new { jobId, status = "Queued", kind = "PassMarkUpdate" });
        }

        private static readonly Dictionary<string, string> PriceEntityMap = new()
        {
            ["Cpu"] = nameof(Cpu),
            ["Gpu"] = nameof(Gpu),
            ["Motherboard"] = nameof(Motherboard),
            ["CpuCooler"] = nameof(CpuCooler),
            ["PcCase"] = nameof(PcCase),
            ["PowerSupply"] = nameof(PowerSupply),
            ["Ram"] = nameof(Ram),
            ["Ssd"] = nameof(Ssd),
            ["Hdd"] = nameof(Hdd),
            ["Fan"] = nameof(Fan),
        };

        [HttpPost("prices/{componentType}")]
        public async Task<IActionResult> UpdatePrices(string componentType, CancellationToken cancellationToken)
        {
            if (!PriceEntityMap.TryGetValue(componentType, out var entityTypeName))
                return BadRequest($"Unknown component type: {componentType}");

            return await EnqueuePriceUpdateAsync(componentType, entityTypeName, cancellationToken);
        }

        private async Task<IActionResult> EnqueuePriceUpdateAsync(string componentType, string entityTypeName, CancellationToken ct)
        {
            if (_tracker.HasActiveJob(componentType))
                return Conflict($"A scrape job for {componentType} is already running or queued.");

            var jobId = Guid.NewGuid();
            var message = new ScrapeJobMessage(jobId, string.Empty, componentType, entityTypeName, "PriceUpdate", null, false);

            _tracker.TrackJob(new ScrapeJobStatus
            {
                JobId = jobId,
                ComponentType = componentType,
                Kind = "PriceUpdate",
                State = "Queued",
                QueuedAt = DateTime.UtcNow
            });

            await _publisher.PublishAsync("scrape-jobs", message, ct);

            return Accepted($"/api/scraper/jobs/{jobId}", new { jobId, status = "Queued", componentType, kind = "PriceUpdate" });
        }

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
                Kind = "Category",
                State = "Queued",
                QueuedAt = DateTime.UtcNow
            });

            await _publisher.PublishAsync("scrape-jobs", message, ct);

            return Accepted($"/api/scraper/jobs/{jobId}", new { jobId, status = "Queued", componentType });
        }

        private async Task<IActionResult> EnqueueSingleAsync(string url, string componentType, string entityTypeName, CancellationToken cancellationToken)
        {
            var jobId = Guid.NewGuid();
            var message = new ScrapeJobMessage(jobId, url, componentType, entityTypeName, "SingleComponent", null, false);

            _tracker.TrackJob(new ScrapeJobStatus
            {
                JobId = jobId,
                ComponentType = componentType,
                Kind = "SingleComponent",
                State = "Queued",
                QueuedAt = DateTime.UtcNow
            });

            await _publisher.PublishAsync("scrape-jobs", message, cancellationToken);

            return Accepted($"/api/scraper/jobs/{jobId}", new { jobId, status = "Queued", componentType });
        }
    }
}
