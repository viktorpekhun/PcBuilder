using Microsoft.EntityFrameworkCore;
using PcBuilder.Contracts.Messages;
using PcBuilder.Persistence.Data;
using PcBuilder.SharedKernel.Caching;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PcBuilder.Api.Services
{
    public class ScrapeResultConsumer : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IScrapeJobTracker _tracker;
        private readonly ICacheInvalidator _cacheInvalidator;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScrapeResultConsumer> _logger;

        public ScrapeResultConsumer(
            IConnection connection,
            IScrapeJobTracker tracker,
            ICacheInvalidator cacheInvalidator,
            IServiceScopeFactory scopeFactory,
            ILogger<ScrapeResultConsumer> logger)
        {
            _connection = connection;
            _tracker = tracker;
            _cacheInvalidator = cacheInvalidator;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CleanupOrphanedJobsAsync(stoppingToken);

            await using var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync("scrape-results", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync("scrape-started", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var result = JsonSerializer.Deserialize<ScrapeJobResultMessage>(json);

                    if (result != null)
                    {
                        _tracker.MarkCompleted(result.JobId, result.Success ? null : result.ErrorMessage, result.ItemsScraped);

                        if (result.Success)
                        {
                            _cacheInvalidator.InvalidateByPrefix($"components:{result.ComponentType}");
                            _logger.LogInformation("Scrape job {JobId} for {ComponentType} completed — cache invalidated", result.JobId, result.ComponentType);
                        }
                        else
                        {
                            _logger.LogWarning("Scrape job {JobId} for {ComponentType} failed: {Error}", result.JobId, result.ComponentType, result.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing scrape result message");
                }
                finally
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
            };

            var startedConsumer = new AsyncEventingBasicConsumer(channel);
            startedConsumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var msg = JsonSerializer.Deserialize<ScrapeJobStartedMessage>(json);

                    if (msg != null)
                    {
                        _tracker.MarkRunning(msg.JobId);
                        _logger.LogInformation("Scrape job {JobId} for {ComponentType} started", msg.JobId, msg.ComponentType);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing scrape started message");
                }
                finally
                {
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
            };

            await channel.BasicConsumeAsync("scrape-results", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await channel.BasicConsumeAsync("scrape-started", autoAck: false, consumer: startedConsumer, cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
        }

        private async Task CleanupOrphanedJobsAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var orphaned = await db.ScrapeJobs
                    .Where(j => j.State == "Running" || j.State == "Queued" || j.State == "Cancelling")
                    .ToListAsync(ct);

                if (orphaned.Count == 0) return;

                var now = DateTime.UtcNow;
                foreach (var job in orphaned)
                {
                    job.State = "Failed";
                    job.CompletedAt = now;
                    job.ErrorMessage = "Job was interrupted by API restart.";
                }

                await db.SaveChangesAsync(ct);
                _logger.LogWarning("Marked {Count} orphaned scrape jobs as Failed on startup.", orphaned.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up orphaned scrape jobs on startup.");
            }
        }
    }
}
