using PcBuilder.Contracts.Messages;
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
        private readonly ILogger<ScrapeResultConsumer> _logger;

        public ScrapeResultConsumer(
            IConnection connection,
            IScrapeJobTracker tracker,
            ICacheInvalidator cacheInvalidator,
            ILogger<ScrapeResultConsumer> logger)
        {
            _connection = connection;
            _tracker = tracker;
            _cacheInvalidator = cacheInvalidator;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync("scrape-results", durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var result = JsonSerializer.Deserialize<ScrapeJobResultMessage>(json);

                    if (result != null)
                    {
                        _tracker.MarkCompleted(result.JobId, result.Success ? null : result.ErrorMessage);

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

            await channel.BasicConsumeAsync("scrape-results", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
        }
    }
}
