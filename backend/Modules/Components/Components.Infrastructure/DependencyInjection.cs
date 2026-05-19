using Components.Application.CollectionFilters;
using Components.Application.Mappers;
using Components.Infrastructure.CollectionFilters;
using Microsoft.Extensions.DependencyInjection;

namespace Components.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddComponentsModule(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
                typeof(Components.Application.Queries.GetComponentsByTypeQuery).Assembly));

            services.AddAutoMapper(typeof(ComponentMappingProfile));

            services.AddScoped<IPowerSupplyCollectionFilterService, PowerSupplyCollectionFilterService>();

            return services;
        }
    }
}
