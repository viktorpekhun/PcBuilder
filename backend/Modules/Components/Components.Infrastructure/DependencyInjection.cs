using Components.Application.Mappers;
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

            return services;
        }
    }
}
