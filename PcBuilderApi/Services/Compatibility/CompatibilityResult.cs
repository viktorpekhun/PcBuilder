using static PcBuilderApi.Utilities.SD;

namespace PcBuilderApi.Services.Compatibility
{
    public class CompatibilityResult
    {
        public bool IsCompatible => Messages.All(m => m.Type != CompatibilityMessageType.Problem);
        public List<CompatibilityMessage> Messages { get; set; } = new();
    }
}
