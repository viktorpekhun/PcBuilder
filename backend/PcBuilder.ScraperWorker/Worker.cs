using Components.Domain.Entities;
using PcBuilder.Contracts.Messages;
using PcBuilder.SharedKernel.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Scraping.Application.Interfaces;
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

        // Separate channel for jobs (with prefetch=1 so we process one job at a time)
        await using var jobChannel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);
        await jobChannel.QueueDeclareAsync("scrape-jobs", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await jobChannel.QueueDeclareAsync("scrape-results", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await jobChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

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
            ScrapeJobMessage? message = null;

            try
            {
                message = JsonSerializer.Deserialize<ScrapeJobMessage>(json);
                if (message == null)
                {
                    _logger.LogWarning("Received null scrape job message, skipping.");
                    await jobChannel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation("Processing scrape job {JobId} for {ComponentType}", message.JobId, message.ComponentType);

                if (!EntityTypeMap.TryGetValue(message.EntityTypeName, out var entityType))
                {
                    _logger.LogWarning("Unknown entity type: {EntityTypeName}", message.EntityTypeName);
                    await PublishResultAsync(jobChannel, message.JobId, message.ComponentType, false, $"Unknown entity type: {message.EntityTypeName}", stoppingToken);
                    await jobChannel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                using var jobCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                _activeJobs[message.JobId] = jobCts;

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scraperService = scope.ServiceProvider.GetRequiredService<ScraperService>();

                    if (message.Kind == "Category")
                    {
                        var method = typeof(ScraperService)
                            .GetMethod(nameof(ScraperService.ScrapeCategoryAsync))!
                            .MakeGenericMethod(entityType);

                        var task = (Task)method.Invoke(scraperService, [message.Url, Enum.Parse<ComponentType>(message.ComponentType), jobCts.Token])!;
                        await task;
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
                    }

                    //if (message.CorrectGpuModels)
                    //{
                    //    var correctionService = scope.ServiceProvider.GetRequiredService<IDataCorrectionService>();
                    //    await correctionService.CorrectGpuModels();
                    //    _logger.LogInformation("GPU model correction completed for job {JobId}", message.JobId);
                    //}

                    //if (message.ComponentType == "PcCase")
                    //{
                    //    var translationService = scope.ServiceProvider.GetRequiredService<IComponentTranslationService>();
                    //    await translationService.TranslatePcCaseFieldsAsync(jobCts.Token);
                    //    _logger.LogInformation("PcCase field translation completed for job {JobId}", message.JobId);
                    //}

                    //if (message.ComponentType == "Ram")
                    //{
                    //    var translationService = scope.ServiceProvider.GetRequiredService<IComponentTranslationService>();
                    //    await translationService.TranslateRamFieldsAsync(jobCts.Token);
                    //    _logger.LogInformation("Ram field translation completed for job {JobId}", message.JobId);
                    //}

                    await PublishResultAsync(jobChannel, message.JobId, message.ComponentType, true, null, stoppingToken);
                    _logger.LogInformation("Scrape job {JobId} for {ComponentType} completed successfully", message.JobId, message.ComponentType);
                }
                catch (OperationCanceledException) when (jobCts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Scrape job {JobId} for {ComponentType} was cancelled", message.JobId, message.ComponentType);
                    await PublishResultAsync(jobChannel, message.JobId, message.ComponentType, false, "Cancelled", stoppingToken);
                }
                finally
                {
                    _activeJobs.TryRemove(message.JobId, out CancellationTokenSource? _);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scrape job {JobId} failed", message?.JobId);
                if (message != null)
                    await PublishResultAsync(jobChannel, message.JobId, message.ComponentType, false, ex.Message, stoppingToken);
            }
            finally
            {
                await jobChannel.BasicAckAsync(ea.DeliveryTag, false);
            }
        };

        await jobChannel.BasicConsumeAsync("scrape-jobs", autoAck: false, consumer: jobConsumer, cancellationToken: stoppingToken);

        // Keep the worker alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
    }

    private static async Task PublishResultAsync(IChannel channel, Guid jobId, string componentType, bool success, string? errorMessage, CancellationToken ct)
    {
        var result = new ScrapeJobResultMessage(jobId, componentType, success, errorMessage, DateTime.UtcNow);
        var json = JsonSerializer.Serialize(result);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties { Persistent = true };
        await channel.BasicPublishAsync("", "scrape-results", mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
    }
}
