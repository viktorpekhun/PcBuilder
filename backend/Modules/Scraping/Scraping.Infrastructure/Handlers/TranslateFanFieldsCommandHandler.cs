using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application.Commands;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Handlers
{
    public class TranslateFanFieldsCommandHandler : IRequestHandler<TranslateFanFieldsCommand>
    {
        private readonly IComponentTranslationService _translationService;
        private readonly ICacheInvalidator _cacheInvalidator;

        public TranslateFanFieldsCommandHandler(
            IComponentTranslationService translationService,
            ICacheInvalidator cacheInvalidator)
        {
            _translationService = translationService;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task Handle(TranslateFanFieldsCommand request, CancellationToken cancellationToken)
        {
            await _translationService.TranslateFanFieldsAsync(cancellationToken);
            _cacheInvalidator.InvalidateByPrefix($"components:{ComponentType.Fan}");
        }
    }
}
