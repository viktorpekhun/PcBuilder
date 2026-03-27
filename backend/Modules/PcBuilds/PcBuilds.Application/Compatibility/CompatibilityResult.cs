using PcBuilder.SharedKernel.Enums;

namespace PcBuilds.Application.Compatibility
{
    public class CompatibilityResult
    {
        public bool IsCompatible => Messages.All(m => m.Type != CompatibilityMessageType.Problem);
        public List<CompatibilityMessage> Messages { get; set; } = new();
    }
}
