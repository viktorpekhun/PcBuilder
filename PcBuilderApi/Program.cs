using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PcBuilderApi.Data;
using PcBuilderApi.Models;
using PcBuilderApi.Repositories.Implementations;
using PcBuilderApi.Repositories.Interfaces;
using PcBuilderApi.Scrapers;
using PcBuilderApi.Scrapers.Implementation;
using PcBuilderApi.Services.Compatibility;
using PcBuilderApi.Services.Compatibility.Rules;
using PcBuilderApi.Services.Implementations;
using PcBuilderApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IComponentScraper<Cpu>, CpuScraper>();
builder.Services.AddScoped<IComponentScraper<Gpu>, GpuScraper>();
builder.Services.AddScoped<IComponentScraper<Motherboard>, MotherboardScraper>();
builder.Services.AddScoped<IComponentScraper<CpuCooler>, CpuCoolerScraper>();
builder.Services.AddScoped<IComponentScraper<PcCase>, PcCaseScraper>();
builder.Services.AddScoped<IComponentScraper<PowerSupply>, PowerSupplyScraper>();
builder.Services.AddScoped<IComponentScraper<Ram>, RamScraper>();
builder.Services.AddScoped<IComponentScraper<Ssd>, SsdScraper>();
builder.Services.AddScoped<IComponentScraper<Hdd>, HddScraper>();
builder.Services.AddScoped<IComponentScraper<Fan>, FanScraper>();

builder.Services.AddHttpClient<IPaginationScraper, PaginationScraper>();
builder.Services.AddScoped<IProxyScraper, ProxyScraper>();

builder.Services.AddScoped<ComponentScraperFactory>();

builder.Services.AddScoped<CompatibilityChecker>();
builder.Services.AddScoped<ScraperService>();
builder.Services.AddScoped<IPcBuildService, PcBuildService>();

builder.Services.AddScoped<ICompatibilityRule, CpuMotherboardSocketRule>();

// Додаємо CORS перед викликом `Build()`
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173") // URL Vite
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "Документація API для мого застосунку"
    });
});

var app = builder.Build();

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

// Використовуємо CORS перед авторизацією
app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();


