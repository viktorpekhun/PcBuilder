using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application.Commands;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Handlers
{
    public class TranslateCpuCoolerFieldsCommandHandler : IRequestHandler<TranslateCpuCoolerFieldsCommand>
    {
        private readonly IComponentTranslationService _translationService;
        private readonly ICacheInvalidator _cacheInvalidator;

        public TranslateCpuCoolerFieldsCommandHandler(
            IComponentTranslationService translationService,
            ICacheInvalidator cacheInvalidator)
        {
            _translationService = translationService;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task Handle(TranslateCpuCoolerFieldsCommand request, CancellationToken cancellationToken)
        {
            await _translationService.TranslateCpuCoolerFieldsAsync(cancellationToken);
            _cacheInvalidator.InvalidateByPrefix($"components:{ComponentType.CpuCooler}");
        }
    }
}
