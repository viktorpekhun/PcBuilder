namespace PcBuilder.SharedKernel.Services
{
    public record TextModerationResult(bool IsToxic, float Score, string Language);

    public interface ITextModerationService
    {
        Task<TextModerationResult> CheckAsync(string text, float threshold = 0.85f, CancellationToken cancellationToken = default);
    }
}
