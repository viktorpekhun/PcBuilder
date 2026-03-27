using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility
{
    public class CompatibilityMessage
    {
        public CompatibilityMessageType Type { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
