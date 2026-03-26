using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Filtering;

namespace Components.Application.Queries
{
    public record GetComponentsByTypeQuery(
        ComponentType ComponentType,
        ResourceParameters Parameters) : IRequest<PagedResponse<object>>, ICacheableQuery
    {
        public string CacheKey => $"components:{ComponentType}:list:{Parameters.ToCacheKey()}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }
}
