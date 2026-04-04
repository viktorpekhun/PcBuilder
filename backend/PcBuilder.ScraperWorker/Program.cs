using PcBuilder.Persistence;
using PcBuilder.ScraperWorker;
using PcBuilder.SharedKernel.Caching;
using RabbitMQ.Client;
using Scraping.Infrastructure;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, config) =>
    config.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddScrapingModule(builder.Configuration);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CacheService>();
builder.Services.AddSingleton<ICacheInvalidator>(sp => sp.GetRequiredService<CacheService>());

var rabbitConfig = builder.Configuration.GetSection("RabbitMq");
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = rabbitConfig["HostName"] ?? "localhost",
        Port = rabbitConfig.GetValue<int>("Port", 5672),
        UserName = rabbitConfig["UserName"] ?? "guest",
        Password = rabbitConfig["Password"] ?? "guest",
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
