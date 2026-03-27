using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Queries
{
    public record GetFilterOptionsQuery(
        ComponentType ComponentType) : IRequest<Dictionary<string, List<string>>>, ICacheableQuery
    {
        public string CacheKey => $"components:{ComponentType}:filters";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
    }
}
