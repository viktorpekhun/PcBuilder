using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Moderation.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddModerationModule(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Moderation.Application.Commands.ReportReviewCommand).Assembly));

            services.AddValidatorsFromAssembly(typeof(Moderation.Application.Commands.ReportReviewCommand).Assembly);

            return services;
        }
    }
}
