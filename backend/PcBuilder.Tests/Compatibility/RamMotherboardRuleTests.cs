using Components.Domain.Entities;
using PcBuilder.SharedKernel.Enums;
using PcBuilds.Application.Compatibility.Rules;
using PcBuilds.Domain.Entities;

namespace PcBuilder.Tests.Compatibility
{
    public class RamMotherboardRuleTests
    {
        private readonly RamMotherboardRule _rule = new();

        [Fact]
        public void Check_NoRamOrMotherboard_ReturnsCompatible()
        {
            var build = new PcBuild { Motherboard = null };
            var result = _rule.Check(build);

            Assert.True(result.IsStrictlyCompatible);
            Assert.Empty(result.Issues);
        }

        [Fact]
        public void Check_MatchingDimmType_ReturnsCompatible()
        {
            var build = new PcBuild
            {
                Motherboard = new Motherboard
                {
                    DimmType = "DDR5",
                    DimmFrequency = 6000,
                    DimmSlots = 4,
                    DimmCapacity = 128
                },
                PcBuild_Rams = new List<PcBuild_Ram>
                {
                    new()
                    {
                        Quantity = 1,
                        Ram = new Ram { Name = "Test RAM", Type = "DDR5", Frequency = 5600, ModuleQuantity = 2, Capacity = 16 }
                    }
                }
            };
            var result = _rule.Check(build);

            Assert.True(result.IsStrictlyCompatible);
            Assert.Empty(result.Issues);
        }

        [Fact]
        public void Check_MismatchedDimmType_ReturnsProblem()
        {
            var build = new PcBuild
            {
                Motherboard = new Motherboard
                {
                    DimmType = "DDR5",
                    DimmFrequency = 6000
                },
                PcBuild_Rams = new List<PcBuild_Ram>
                {
                    new()
                    {
                        Quantity = 1,
                        Ram = new Ram { Name = "Test RAM", Type = "DDR4", Frequency = 3200, ModuleQuantity = 2, Capacity = 16 }
                    }
                }
            };
            var result = _rule.Check(build);

            Assert.False(result.IsStrictlyCompatible);
            Assert.Contains(result.Issues, i => i.Severity == CompatibilitySeverity.Critical);
        }

        [Fact]
        public void Check_FrequencyTooHigh_ReturnsProblem()
        {
            var build = new PcBuild
            {
                Motherboard = new Motherboard
                {
                    DimmType = "DDR5",
                    DimmFrequency = 5200
                },
                PcBuild_Rams = new List<PcBuild_Ram>
                {
                    new()
                    {
                        Quantity = 1,
                        Ram = new Ram { Name = "Fast RAM", Type = "DDR5", Frequency = 6000, ModuleQuantity = 2, Capacity = 16 }
                    }
                }
            };
            var result = _rule.Check(build);

            Assert.False(result.IsStrictlyCompatible);
            Assert.Contains(result.Issues, i => i.Severity == CompatibilitySeverity.Critical);
        }

        [Fact]
        public void Check_TooManyModules_ReturnsWarning()
        {
            var build = new PcBuild
            {
                Motherboard = new Motherboard
                {
                    DimmType = "DDR5",
                    DimmFrequency = 6000,
                    DimmSlots = 2,
                    DimmCapacity = 128
                },
                PcBuild_Rams = new List<PcBuild_Ram>
                {
                    new()
                    {
                        Quantity = 1,
                        Ram = new Ram { Name = "RAM Kit", Type = "DDR5", Frequency = 5600, ModuleQuantity = 2, Capacity = 16 }
                    },
                    new()
                    {
                        Quantity = 1,
                        Ram = new Ram { Name = "RAM Kit 2", Type = "DDR5", Frequency = 5600, ModuleQuantity = 2, Capacity = 16 }
                    }
                }
            };
            var result = _rule.Check(build);

            Assert.True(result.IsStrictlyCompatible);
            Assert.Contains(result.Issues, i => i.Severity == CompatibilitySeverity.Warning);
        }

        [Fact]
        public void Check_DifferentFrequencies_ReturnsWarning()
        {
            var build = new PcBuild
            {
                Motherboard = new Motherboard
                {
                    DimmType = "DDR5",
                    DimmFrequency = 6000,
                    DimmSlots = 4,
                    DimmCapacity = 128
                },
                PcBuild_Rams = new List<PcBuild_Ram>
                {
                    new()
                    {
                        Quantity = 1,
                        Ram = new Ram { Name = "RAM A", Type = "DDR5", Frequency = 5600, ModuleQuantity = 2, Capacity = 16 }
                    },
                    new()
                    {
                        Quantity = 1,
                        Ram = new Ram { Name = "RAM B", Type = "DDR5", Frequency = 4800, ModuleQuantity = 2, Capacity = 16 }
                    }
                }
            };
            var result = _rule.Check(build);

            Assert.True(result.IsStrictlyCompatible);
            Assert.Contains(result.Issues, i => i.Severity == CompatibilitySeverity.Warning);
        }
    }
}
