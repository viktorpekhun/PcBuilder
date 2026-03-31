using Components.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scraping.Application.Interfaces;
using Scraping.Infrastructure.Configuration;
using Scraping.Infrastructure.Handlers;
using Scraping.Infrastructure.Scrapers;
using Scraping.Infrastructure.Services;

namespace Scraping.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddScrapingModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureTranslatorOptions>(configuration.GetSection(AzureTranslatorOptions.SectionName));
            services.AddHttpClient<ITranslationService, AzureTranslatorService>();

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ScrapeCategoryCommandHandler).Assembly));

            services.AddScoped<IComponentScraper<Cpu>, CpuScraper>();
            services.AddScoped<IComponentScraper<Gpu>, GpuScraper>();
            services.AddScoped<IComponentScraper<Motherboard>, MotherboardScraper>();
            services.AddScoped<IComponentScraper<CpuCooler>, CpuCoolerScraper>();
            services.AddScoped<IComponentScraper<PcCase>, PcCaseScraper>();
            services.AddScoped<IComponentScraper<PowerSupply>, PowerSupplyScraper>();
            services.AddScoped<IComponentScraper<Ram>, RamScraper>();
            services.AddScoped<IComponentScraper<Ssd>, SsdScraper>();
            services.AddScoped<IComponentScraper<Hdd>, HddScraper>();
            services.AddScoped<IComponentScraper<Fan>, FanScraper>();

            services.AddHttpClient<IPaginationScraper, PaginationScraper>();
            services.AddScoped<IProxyScraper, ProxyScraper>();
            services.AddSingleton<ProxyPool>();
            services.AddScoped<ComponentScraperFactory>();
            services.AddScoped<ScraperService>();
            services.AddScoped<IDataCorrectionService, DataCorrectionService>();

            return services;
        }
    }
}
