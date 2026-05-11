using Components.Application.PreFilter;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PcBuilds.Application.AutoBuilder;
using PcBuilds.Application.Compatibility;
using PcBuilds.Application.Compatibility.PreFilter;
using PcBuilds.Application.Compatibility.Rules;
using PcBuilds.Infrastructure.AutoBuilder;
using PcBuilds.Infrastructure.Compatibility.PreFilter;

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

            services.AddScoped<ComponentCompatibilityFilter>();
            services.AddScoped<IComponentCompatibilityFilter>(sp => sp.GetRequiredService<ComponentCompatibilityFilter>());
            services.AddScoped<IComponentQueryPreFilter>(sp => sp.GetRequiredService<ComponentCompatibilityFilter>());

            services.AddSingleton<IScenarioPolicyProvider, ScenarioPolicyProvider>();
            services.AddScoped<ICandidatePruner, EfCoreCandidatePruner>();
            services.AddScoped<ICoolerSelector, CoolerSelector>();
            services.AddScoped<BudgetReserveCalculator>();
            services.AddScoped<IBuildAssembler, BuildAssembler>();
            services.AddScoped<IPcBuildGalleryMapper, PcBuildGalleryMapper>();
            services.AddScoped<IAutoBuilderService, GreedyAutoBuilderService>();

            return services;
        }
    }
}
