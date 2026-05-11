using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Notifications.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Notifications.Application.Commands.CreateNotificationCommand).Assembly));

            services.AddValidatorsFromAssembly(typeof(Notifications.Application.Commands.CreateNotificationCommand).Assembly);

            return services;
        }
    }
}
