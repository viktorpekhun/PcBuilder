using Components.Application.PreFilter;
using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Filtering;

namespace Components.Application.Queries
{
    public record GetComponentsByTypeQuery(
        ComponentType ComponentType,
        ResourceParameters Parameters,
        bool ApplyCompatibilityPrefilter = false,
        PartialBuildIds? PartialBuildIds = null) : IRequest<Result<PagedResponse<object>>>, ICacheableQuery
    {
        public string CacheKey =>
            $"components:{ComponentType}:list:{Parameters.ToCacheKey()}" +
            (ApplyCompatibilityPrefilter && PartialBuildIds != null
                ? $":pf:{PartialBuildIds.CpuId}:{PartialBuildIds.GpuId}:{PartialBuildIds.MotherboardId}:{PartialBuildIds.RamId}:{PartialBuildIds.CpuCoolerId}:{PartialBuildIds.PcCaseId}:{PartialBuildIds.PowerSupplyId}"
                : string.Empty);

        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }
}
