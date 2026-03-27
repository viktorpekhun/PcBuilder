using MediatR;
using PcBuilder.SharedKernel.Caching;
using Scraping.Application.Commands;
using Scraping.Infrastructure.Services;

namespace Scraping.Infrastructure.Handlers
{
    public class ScrapeCategoryCommandHandler : IRequestHandler<ScrapeCategoryCommand>
    {
        private readonly ScraperService _scraperService;
        private readonly ICacheInvalidator _cacheInvalidator;

        public ScrapeCategoryCommandHandler(ScraperService scraperService, ICacheInvalidator cacheInvalidator)
        {
            _scraperService = scraperService;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task Handle(ScrapeCategoryCommand request, CancellationToken cancellationToken)
        {
            var method = typeof(ScraperService)
                .GetMethod(nameof(ScraperService.ScrapeCategoryAsync))!
                .MakeGenericMethod(request.EntityType);

            var task = (Task)method.Invoke(_scraperService, new object[] { request.CategoryUrl, request.ComponentType })!;
            await task;

            _cacheInvalidator.InvalidateByPrefix($"components:{request.ComponentType}");
        }
    }
}
