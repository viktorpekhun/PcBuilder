namespace Scraping.Application.Interfaces
{
    public interface IComponentTranslationService
    {
        Task TranslatePcCaseFieldsAsync(CancellationToken cancellationToken = default);
        Task TranslateFanFieldsAsync(CancellationToken cancellationToken = default);
        Task TranslatePowerSupplyFieldsAsync(CancellationToken cancellationToken = default);
        Task TranslateRamFieldsAsync(CancellationToken cancellationToken = default);
        Task TranslateCpuCoolerFieldsAsync(CancellationToken cancellationToken = default);
    }
}
