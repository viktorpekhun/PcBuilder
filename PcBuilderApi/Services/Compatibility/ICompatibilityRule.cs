using PcBuilderApi.Models;

namespace PcBuilderApi.Services.Compatibility
{

    public interface ICompatibilityRule
    {
        string Name { get; }
        CompatibilityResult Check(PcBuild pcBuild);
    }
}
