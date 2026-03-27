using Auth.Application.Interfaces;
using Auth.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Auth.Application.Commands.RegisterCommand).Assembly));

            services.AddValidatorsFromAssembly(typeof(Auth.Application.Commands.RegisterCommand).Assembly);

            services.AddSingleton<ITokenProviderService, TokenProviderService>();

            return services;
        }
    }
}
