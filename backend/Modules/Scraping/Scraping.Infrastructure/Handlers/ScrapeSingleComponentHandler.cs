using MediatR;
using PcBuilder.SharedKernel.Caching;
using Scraping.Application.Commands;
using Scraping.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraping.Infrastructure.Handlers
{
    public class ScrapeSingleComponentHandler : IRequestHandler<ScrapeSingleComponentCommand>
    {
        private readonly ScraperService _scraperService;

        public ScrapeSingleComponentHandler(ScraperService scraperService)
        {
            _scraperService = scraperService;
        }
        public async Task Handle(ScrapeSingleComponentCommand request, CancellationToken cancellationToken)
        {
            var method = typeof(ScraperService)
                .GetMethod(nameof(ScraperService.ScrapeSingleComponentAsync))!
                .MakeGenericMethod(request.EntityType);

            var task = (Task)method.Invoke(_scraperService, new object[] { request.ComponentUrl, request.ComponentType })!;
            await task;
        }
    }
}
