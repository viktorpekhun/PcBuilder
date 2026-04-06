using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application.Commands;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Handlers
{
    public class TranslateRamFieldsCommandHandler : IRequestHandler<TranslateRamFieldsCommand>
    {
        private readonly IComponentTranslationService _translationService;
        private readonly ICacheInvalidator _cacheInvalidator;

        public TranslateRamFieldsCommandHandler(
            IComponentTranslationService translationService,
            ICacheInvalidator cacheInvalidator)
        {
            _translationService = translationService;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task Handle(TranslateRamFieldsCommand request, CancellationToken cancellationToken)
        {
            await _translationService.TranslateRamFieldsAsync(cancellationToken);
            _cacheInvalidator.InvalidateByPrefix($"components:{ComponentType.Ram}");
        }
    }
}
