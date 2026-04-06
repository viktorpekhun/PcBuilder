using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using Scraping.Application.Commands;
using Scraping.Application.Interfaces;

namespace Scraping.Infrastructure.Handlers
{
    public class TranslatePowerSupplyFieldsCommandHandler : IRequestHandler<TranslatePowerSupplyFieldsCommand>
    {
        private readonly IComponentTranslationService _translationService;
        private readonly ICacheInvalidator _cacheInvalidator;

        public TranslatePowerSupplyFieldsCommandHandler(
            IComponentTranslationService translationService,
            ICacheInvalidator cacheInvalidator)
        {
            _translationService = translationService;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task Handle(TranslatePowerSupplyFieldsCommand request, CancellationToken cancellationToken)
        {
            await _translationService.TranslatePowerSupplyFieldsAsync(cancellationToken);
            _cacheInvalidator.InvalidateByPrefix($"components:{ComponentType.PowerSupply}");
        }
    }
}
