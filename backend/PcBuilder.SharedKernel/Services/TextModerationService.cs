using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace PcBuilder.SharedKernel.Services
{
    public class TextModerationService : ITextModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TextModerationService> _logger;

        public TextModerationService(HttpClient httpClient, ILogger<TextModerationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TextModerationResult> CheckAsync(string text, float threshold = 0.85f, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new PredictRequest(text, threshold);
                var response = await _httpClient.PostAsJsonAsync("/predict", request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var payload = await response.Content.ReadFromJsonAsync<PredictResponse>(cancellationToken);
                if (payload is null)
                {
                    _logger.LogWarning("Text moderation API returned empty body");
                    return new TextModerationResult(false, 0f, "unknown");
                }

                return new TextModerationResult(payload.IsToxic, payload.Score, payload.Language);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Text moderation API unreachable, allowing text to pass");
                return new TextModerationResult(false, 0f, "unknown");
            }
        }

        private record PredictRequest(string Text, float Threshold);

        private class PredictResponse
        {
            [JsonPropertyName("is_toxic")]
            public bool IsToxic { get; set; }

            [JsonPropertyName("score")]
            public float Score { get; set; }

            [JsonPropertyName("language")]
            public string Language { get; set; } = "unknown";
        }
    }
}
