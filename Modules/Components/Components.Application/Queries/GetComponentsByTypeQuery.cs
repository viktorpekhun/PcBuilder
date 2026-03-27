using Components.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;
using PcBuilder.SharedKernel.Filtering;

namespace Components.Application.Queries
{
    public record GetComponentsByTypeQuery(
        ComponentType ComponentType,
        ResourceParameters Parameters) : IRequest<Result<PagedResponse<IComponentListDto>>>, ICacheableQuery
    {
        public string CacheKey => $"components:{ComponentType}:list:{Parameters.ToCacheKey()}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }
}
