using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Application.Compatibility.Rules;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Tests.Compatibility
{
    public class CpuMotherboardSocketRuleTests
    {
        private readonly CpuMotherboardSocketRule _rule = new();

        [Fact]
        public void Check_BothNull_ReturnsCompatible()
        {
            var build = new PcBuild { Cpu = null, Motherboard = null };
            var result = _rule.Check(build);

            Assert.True(result.IsStrictlyCompatible);
            Assert.Empty(result.Issues);
        }

        [Fact]
        public void Check_CpuNull_ReturnsCompatible()
        {
            var build = new PcBuild
            {
                Cpu = null,
                Motherboard = new Motherboard { Socket = "AM5" }
            };
            var result = _rule.Check(build);

            Assert.True(result.IsStrictlyCompatible);
            Assert.Empty(result.Issues);
        }

        [Fact]
        public void Check_MatchingSockets_ReturnsCompatible()
        {
            var build = new PcBuild
            {
                Cpu = new Cpu { Socket = "AM5" },
                Motherboard = new Motherboard { Socket = "AM5" }
            };
            var result = _rule.Check(build);

            Assert.True(result.IsStrictlyCompatible);
            Assert.Empty(result.Issues);
        }

        [Fact]
        public void Check_MismatchedSockets_ReturnsProblem()
        {
            var build = new PcBuild
            {
                Cpu = new Cpu { Socket = "AM5" },
                Motherboard = new Motherboard { Socket = "LGA1700" }
            };
            var result = _rule.Check(build);

            Assert.False(result.IsStrictlyCompatible);
            Assert.Single(result.Issues);
            Assert.Equal(CompatibilitySeverity.Critical, result.Issues[0].Severity);
        }

        [Fact]
        public void Check_EmptySockets_ReturnsWarning()
        {
            var build = new PcBuild
            {
                Cpu = new Cpu { Socket = "" },
                Motherboard = new Motherboard { Socket = "AM5" }
            };
            var result = _rule.Check(build);

            Assert.True(result.IsStrictlyCompatible);
            Assert.Single(result.Issues);
            Assert.Equal(CompatibilitySeverity.Warning, result.Issues[0].Severity);
        }
    }
}
