using PcBuilds.Domain.Entities;

namespace PcBuilds.Application.Compatibility
{
    public class CompatibilityChecker
    {
        private readonly List<ICompatibilityRule> _rules;

        public CompatibilityChecker(IEnumerable<ICompatibilityRule> rules)
        {
            _rules = rules.ToList();
        }

        public List<CompatibilityResult> CheckAll(PcBuild pcBuild)
        {
            return _rules.Select(rule => rule.Check(pcBuild))
                 .Where(result => result.Messages.Any())
                 .ToList();
        }
    }
}
