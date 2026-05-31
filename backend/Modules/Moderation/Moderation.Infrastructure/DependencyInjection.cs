using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moderation.Application.Services;
using Moderation.Infrastructure.Services;

namespace Moderation.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddModerationModule(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Moderation.Application.Commands.ReportReviewCommand).Assembly));

            services.AddValidatorsFromAssembly(typeof(Moderation.Application.Commands.ReportReviewCommand).Assembly);

            services.AddScoped<IAdminActivityLogger, AdminActivityLogger>();

            return services;
        }
    }
}
