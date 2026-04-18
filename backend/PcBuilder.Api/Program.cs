using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PcBuilder.Api.Hubs;
using PcBuilder.Api.Middleware;
using PcBuilder.Api.RealTime;
using PcBuilder.Api.Services;
using PcBuilder.Persistence;
using Components.Infrastructure;
using Auth.Infrastructure;
using PcBuilds.Infrastructure;
using Scraping.Infrastructure;
using Moderation.Infrastructure;
using Notifications.Infrastructure;
using MediatR;
using PcBuilder.SharedKernel.Behaviors;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Services;
using RabbitMQ.Client;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new PcBuilder.Api.Middleware.UtcDateTimeConverter());
    });
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddComponentsModule();
builder.Services.AddAuthModule();
builder.Services.AddPcBuildsModule();
builder.Services.AddScrapingModule(builder.Configuration);
builder.Services.AddModerationModule();
builder.Services.AddNotificationsModule();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddSignalR().AddJsonProtocol(o =>
{
    o.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.PayloadSerializerOptions.Converters.Add(new PcBuilder.Api.Middleware.UtcDateTimeConverter());
});
builder.Services.AddTransient<MediatR.INotificationHandler<Notifications.Application.Events.NotificationCreatedEvent>, NotificationSignalRPublisher>();
builder.Services.AddHttpClient<ITextModerationService, TextModerationService>(client =>
{
    var baseUrl = builder.Configuration["TextModeration:BaseUrl"]
        ?? "http://localhost:8001";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CacheService>();
builder.Services.AddSingleton<ICacheInvalidator>(sp => sp.GetRequiredService<CacheService>());

var rabbitConfig = builder.Configuration.GetSection("RabbitMq");
builder.Services.AddSingleton<IConnection>(_ =>
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
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddSingleton<IScrapeJobTracker, DbScrapeJobTracker>();
builder.Services.AddHostedService<ScrapeResultConsumer>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddOpenApi();

var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = rateLimitConfig.GetValue<int>("Auth:PermitLimit");
        opt.Window = TimeSpan.FromMinutes(rateLimitConfig.GetValue<int>("Auth:WindowMinutes"));
        opt.QueueLimit = rateLimitConfig.GetValue<int>("Auth:QueueLimit");
    });
    options.AddFixedWindowLimiter("scraper", opt =>
    {
        opt.PermitLimit = rateLimitConfig.GetValue<int>("Scraper:PermitLimit");
        opt.Window = TimeSpan.FromMinutes(rateLimitConfig.GetValue<int>("Scraper:WindowMinutes"));
        opt.QueueLimit = rateLimitConfig.GetValue<int>("Scraper:QueueLimit");
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("X-Pagination"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PcBuilder API",
        Version = "v1",
        Description = "API documentation for PcBuilder App"
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Enter your token directly (no need to type 'Bearer ')"
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration.GetSection("Jwt:Secret").Value!)),
        ValidateAudience = false,
        ValidateIssuer = false,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = JwtRegisteredClaimNames.Sub
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var token = ctx.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) &&
                ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            {
                ctx.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await AdminSeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    });
}

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapFallbackToFile("/index.html");

app.Run();


