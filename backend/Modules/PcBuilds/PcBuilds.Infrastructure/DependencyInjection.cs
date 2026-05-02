using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.Compatibility;
using PcBuilds.Application.Compatibility.Rules;
using PcBuilds.Infrastructure.AutoBuilder;

namespace PcBuilds.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPcBuildsModule(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(PcBuilds.Application.Commands.SaveBuildCommand).Assembly));

            services.AddValidatorsFromAssembly(typeof(PcBuilds.Application.Commands.SaveBuildCommand).Assembly);

            services.AddScoped<CompatibilityChecker>();

            services.AddScoped<ICompatibilityRule, CpuMotherboardSocketRule>();
            services.AddScoped<ICompatibilityRule, CpuCoolerCpuPowerDissipationRule>();
            services.AddScoped<ICompatibilityRule, CpuCoolerMotherboardSocketRule>();
            services.AddScoped<ICompatibilityRule, CpuCoolerPcCaseSizeRule>();
            services.AddScoped<ICompatibilityRule, GpuMotherboardPcleRule>();
            services.AddScoped<ICompatibilityRule, HddMotherboardSocketRule>();
            services.AddScoped<ICompatibilityRule, SsdMotherboardSocketRule>();
            services.AddScoped<ICompatibilityRule, FanPcCaseCompatibilityRule>();
            services.AddScoped<ICompatibilityRule, FanMotherboardSocketRule>();
            services.AddScoped<ICompatibilityRule, RamMotherboardRule>();
            services.AddScoped<ICompatibilityRule, PcCaseGpuLengthRule>();
            services.AddScoped<ICompatibilityRule, PowerSupplyGpuConnectorRule>();
            services.AddScoped<ICompatibilityRule, PowerSupplyCpuConnectorRule>();
            services.AddScoped<ICompatibilityRule, RequiredPowerSupplyRule>();
            services.AddScoped<ICompatibilityRule, CpuGpuBottleneckRule>();

            services.AddScoped<IAutoBuilderService, GreedyAutoBuilderService>();

            return services;
        }
    }
}
