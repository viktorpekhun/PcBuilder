using Components.Domain.Entities;
using Components.Domain.ValueObjects;
using PcBuilder.Contracts.Messages;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Scraping.Application.Interfaces;
using Scraping.Infrastructure.Scrapers.PassMark;
using Scraping.Infrastructure.Services;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace PcBuilder.ScraperWorker;

public class Worker : BackgroundService
{
    private static readonly Dictionary<string, Type> EntityTypeMap = new()
    {
        ["Cpu"] = typeof(Cpu),
        ["Gpu"] = typeof(Gpu),
        ["Motherboard"] = typeof(Motherboard),
        ["CpuCooler"] = typeof(CpuCooler),
        ["PcCase"] = typeof(PcCase),
        ["PowerSupply"] = typeof(PowerSupply),
        ["Ram"] = typeof(Ram),
        ["Ssd"] = typeof(Ssd),
        ["Hdd"] = typeof(Hdd),
        ["Fan"] = typeof(Fan),
    };

    private static readonly Dictionary<string, Type[]> NestedTypeMap = new()
    {
        ["PowerSupply"] = [typeof(PowerSupplyPowerConnector)],
        ["CpuCooler"] = [typeof(CpuCoolerSocket)],
        ["PcCase"] = [typeof(PcCaseFormFactor), typeof(PcCaseFanLocation)],
        ["Gpu"] = [typeof(GpuPowerConnector)],
        ["Motherboard"] = [typeof(CpuPowerConnector), typeof(RearPort), typeof(InnerPort), typeof(M2Slot), typeof(PcleSlot)],
    };

    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _activeJobs = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _rabbitConnection;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceScopeFactory scopeFactory, IConnection rabbitConnection, ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _rabbitConnection = rabbitConnection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scraper Worker started, waiting for jobs...");

        // Consumer channel for jobs. We ack immediately on receipt and run the actual
        // scrape on a background Task, so the consumer ack timeout is never hit.
        await using var jobChannel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);
        await jobChannel.QueueDeclareAsync("scrape-jobs", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await jobChannel.QueueDeclareAsync("scrape-results", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await jobChannel.QueueDeclareAsync("scrape-started", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await jobChannel.QueueDeclareAsync("scrape-progress", durable: false, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        // Dedicated publisher channel for started/result messages. Not subject to the
        // consumer ack timeout, so long-running jobs can still report completion.
        await using var publishChannel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);
        var publishLock = new SemaphoreSlim(1, 1);

        // Separate channel for cancellations — must not be blocked by job channel's QoS
        await using var cancelChannel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);
        await cancelChannel.QueueDeclareAsync("scrape-cancellations", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        // Listen for cancellation messages on its own channel
        var cancelConsumer = new AsyncEventingBasicConsumer(cancelChannel);
        cancelConsumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var cancelMessage = JsonSerializer.Deserialize<ScrapeJobCancelMessage>(json);

                if (cancelMessage != null && _activeJobs.TryGetValue(cancelMessage.JobId, out var cts))
                {
                    _logger.LogInformation("Cancelling scrape job {JobId}", cancelMessage.JobId);
                    await cts.CancelAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cancellation message");
            }
            finally
            {
                await cancelChannel.BasicAckAsync(ea.DeliveryTag, false);
            }
        };
        await cancelChannel.BasicConsumeAsync("scrape-cancellations", autoAck: false, consumer: cancelConsumer, cancellationToken: stoppingToken);

        // Listen for scrape job messages on the job channel
        var jobConsumer = new AsyncEventingBasicConsumer(jobChannel);
        jobConsumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            ScrapeJobMessage? message;

            try
            {
                message = JsonSerializer.Deserialize<ScrapeJobMessage>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize scrape job message, dropping.");
                await jobChannel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            // Ack immediately — actual work runs on a background task so the broker
            // never sees the 30-min consumer timeout, regardless of how long scraping takes.
            await jobChannel.BasicAckAsync(ea.DeliveryTag, false);

            if (message == null)
            {
                _logger.LogWarning("Received null scrape job message, skipping.");
                return;
            }

            _ = Task.Run(() => RunJobAsync(message, publishChannel, publishLock, stoppingToken), stoppingToken);
        };

        await jobChannel.BasicConsumeAsync("scrape-jobs", autoAck: false, consumer: jobConsumer, cancellationToken: stoppingToken);

        // Keep the worker alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
    }

    private async Task RunJobAsync(ScrapeJobMessage message, IChannel publishChannel, SemaphoreSlim publishLock, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Processing scrape job {JobId} for {ComponentType}", message.JobId, message.ComponentType);

        // PassMarkUpdate does not target a single component type — skip entity-type resolution.
        Type? entityType = null;
        if (message.Kind != "PassMarkUpdate")
        {
            if (!EntityTypeMap.TryGetValue(message.EntityTypeName, out entityType))
            {
                _logger.LogWarning("Unknown entity type: {EntityTypeName}", message.EntityTypeName);
                await PublishResultAsync(publishChannel, publishLock, message.JobId, message.ComponentType, false, $"Unknown entity type: {message.EntityTypeName}", 0, stoppingToken);
                return;
            }
        }

        using var jobCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _activeJobs[message.JobId] = jobCts;

        int itemsScraped = 0;

        try
        {
            await PublishStartedAsync(publishChannel, publishLock, message.JobId, message.ComponentType, stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var scraperService = scope.ServiceProvider.GetRequiredService<ScraperService>();

            Func<int, int?, Task> progressCallback = async (scraped, total) =>
            {
                if (scraped % 5 != 0 && scraped != total) return;
                try { await PublishProgressAsync(publishChannel, publishLock, message.JobId, message.ComponentType, scraped, total, stoppingToken); }
                catch { /* non-fatal */ }
            };

            if (message.Kind == "Category")
            {
                var method = typeof(ScraperService)
                    .GetMethod(nameof(ScraperService.ScrapeCategoryAsync))!
                    .MakeGenericMethod(entityType);

                var task = (Task<int>)method.Invoke(scraperService, [message.Url, Enum.Parse<ComponentType>(message.ComponentType), progressCallback, jobCts.Token])!;
                itemsScraped = await task;

                if (message.CorrectGpuModels)
                {
                    var correctionService = scope.ServiceProvider.GetRequiredService<IDataCorrectionService>();
                    await correctionService.CorrectGpuModels();
                }
            }
            else if (message.Kind == "SingleComponent")
            {
                var nestedTypes = message.NestedTypeNames?
                    .Select(n => NestedTypeMap.TryGetValue(message.EntityTypeName, out var types) ? types : Array.Empty<Type>())
                    .SelectMany(t => t)
                    .Distinct()
                    .ToArray();

                var method = typeof(ScraperService)
                    .GetMethod(nameof(ScraperService.ScrapeSingleComponentAsync))!
                    .MakeGenericMethod(entityType);

                var task = (Task)method.Invoke(scraperService, [message.Url, Enum.Parse<ComponentType>(message.ComponentType), nestedTypes, jobCts.Token])!;
                await task;
                itemsScraped = 1;
            }
            else if (message.Kind == "PriceUpdate")
            {
                var method = typeof(ScraperService)
                    .GetMethod(nameof(ScraperService.UpdatePricesAsync))!
                    .MakeGenericMethod(entityType);

                var task = (Task<int>)method.Invoke(scraperService, [Enum.Parse<ComponentType>(message.ComponentType), progressCallback, jobCts.Token])!;
                itemsScraped = await task;

                var cacheInvalidator = scope.ServiceProvider.GetRequiredService<ICacheInvalidator>();
                cacheInvalidator.InvalidateByPrefix($"components:{message.ComponentType}");
            }
            else if (message.Kind == "PassMarkUpdate")
            {
                var handler = scope.ServiceProvider.GetRequiredService<PassMarkUpdateJobHandler>();
                await handler.RunAsync(jobCts.Token);
            }

            await PublishResultAsync(publishChannel, publishLock, message.JobId, message.ComponentType, true, null, itemsScraped, stoppingToken);
            _logger.LogInformation("Scrape job {JobId} for {ComponentType} completed successfully", message.JobId, message.ComponentType);
        }
        catch (OperationCanceledException) when (jobCts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Scrape job {JobId} for {ComponentType} was cancelled", message.JobId, message.ComponentType);
            await PublishResultAsync(publishChannel, publishLock, message.JobId, message.ComponentType, false, "Cancelled", itemsScraped, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scrape job {JobId} failed", message.JobId);
            await PublishResultAsync(publishChannel, publishLock, message.JobId, message.ComponentType, false, ex.Message, itemsScraped, stoppingToken);
        }
        finally
        {
            _activeJobs.TryRemove(message.JobId, out _);
        }
    }

    private static async Task PublishResultAsync(IChannel channel, SemaphoreSlim publishLock, Guid jobId, string componentType, bool success, string? errorMessage, int itemsScraped, CancellationToken ct)
    {
        var result = new ScrapeJobResultMessage(jobId, componentType, success, errorMessage, DateTime.UtcNow, itemsScraped);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
        var props = new BasicProperties { Persistent = true };

        await publishLock.WaitAsync(ct);
        try
        {
            await channel.BasicPublishAsync("", "scrape-results", mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
        }
        finally
        {
            publishLock.Release();
        }
    }

    private static async Task PublishProgressAsync(IChannel channel, SemaphoreSlim publishLock, Guid jobId, string componentType, int itemsScraped, int? totalItems, CancellationToken ct)
    {
        var msg = new ScrapeJobProgressMessage(jobId, componentType, itemsScraped, totalItems);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        var props = new BasicProperties { Persistent = false };

        await publishLock.WaitAsync(ct);
        try
        {
            await channel.BasicPublishAsync("", "scrape-progress", mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
        }
        finally
        {
            publishLock.Release();
        }
    }

    private static async Task PublishStartedAsync(IChannel channel, SemaphoreSlim publishLock, Guid jobId, string componentType, CancellationToken ct)
    {
        var msg = new ScrapeJobStartedMessage(jobId, componentType, DateTime.UtcNow);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        var props = new BasicProperties { Persistent = true };

        await publishLock.WaitAsync(ct);
        try
        {
            await channel.BasicPublishAsync("", "scrape-started", mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
        }
        finally
        {
            publishLock.Release();
        }
    }
}
