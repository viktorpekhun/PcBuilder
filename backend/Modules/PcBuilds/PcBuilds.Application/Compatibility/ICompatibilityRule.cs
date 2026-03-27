using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility
{
    public interface ICompatibilityRule
    {
        string Name { get; }
        CompatibilityResult Check(PcBuild pcBuild);
    }
}
