using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility
{
    public class CompatibilityMessage
    {
        public CompatibilityMessageType Type { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
