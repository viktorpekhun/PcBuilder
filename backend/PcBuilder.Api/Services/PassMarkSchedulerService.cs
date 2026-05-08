using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PcBuilder.Api.Services
{
    public class PassMarkSchedulerService : BackgroundService
    {
        private readonly IConnection _rabbitConnection;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<PassMarkSchedulerService> _logger;

        public PassMarkSchedulerService(
            IConnection rabbitConnection,
            IConfiguration configuration,
            IHostEnvironment environment,
            ILogger<PassMarkSchedulerService> logger)
        {
            _rabbitConnection = rabbitConnection;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("PassMark scheduler is disabled in Development — use the endpoint to trigger manually.");
                return;
            }

            var intervalDays = _configuration.GetValue<int>("PassMark:UpdateIntervalDays", 7);
            var interval = TimeSpan.FromDays(intervalDays);

            _logger.LogInformation("PassMark scheduler started — will enqueue every {Days} day(s).", intervalDays);

            while (!stoppingToken.IsCancellationRequested)
            {
                await EnqueueAsync(stoppingToken);
                await Task.Delay(interval, stoppingToken).ContinueWith(_ => { }, CancellationToken.None);
            }
        }

        private async Task EnqueueAsync(CancellationToken ct)
        {
            try
            {
                await using var channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: ct);
                await channel.QueueDeclareAsync("scrape-jobs", durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);

                var message = new { Kind = "PassMarkUpdate", JobId = Guid.NewGuid() };
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var props = new BasicProperties { Persistent = true };

                await channel.BasicPublishAsync("", "scrape-jobs", mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
                _logger.LogInformation("PassMark update job enqueued.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue PassMark update job.");
            }
        }
    }
}
