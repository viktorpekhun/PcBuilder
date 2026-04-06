using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application.Commands;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Handlers
{
    public class TranslatePcCaseFieldsCommandHandler : IRequestHandler<TranslatePcCaseFieldsCommand>
    {
        private readonly IComponentTranslationService _translationService;
        private readonly ICacheInvalidator _cacheInvalidator;

        public TranslatePcCaseFieldsCommandHandler(
            IComponentTranslationService translationService,
            ICacheInvalidator cacheInvalidator)
        {
            _translationService = translationService;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task Handle(TranslatePcCaseFieldsCommand request, CancellationToken cancellationToken)
        {
            await _translationService.TranslatePcCaseFieldsAsync(cancellationToken);
            _cacheInvalidator.InvalidateByPrefix($"components:{ComponentType.PcCase}");
        }
    }
}
