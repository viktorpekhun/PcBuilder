using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application.Commands;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Handlers
{
    public class CorrectGpuModelsCommandHandler : IRequestHandler<CorrectGpuModelsCommand>
    {
        private readonly IDataCorrectionService _dataCorrectionService;
        private readonly ICacheInvalidator _cacheInvalidator;

        public CorrectGpuModelsCommandHandler(
            IDataCorrectionService dataCorrectionService,
            ICacheInvalidator cacheInvalidator)
        {
            _dataCorrectionService = dataCorrectionService;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task Handle(CorrectGpuModelsCommand request, CancellationToken cancellationToken)
        {
            await _dataCorrectionService.CorrectGpuModels();
            _cacheInvalidator.InvalidateByPrefix($"components:{ComponentType.Gpu}");
        }
    }
}
