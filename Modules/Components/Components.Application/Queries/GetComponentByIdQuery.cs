using MediatR;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Queries
{
    public record GetComponentByIdQuery(
        Guid Id,
        ComponentType ComponentType) : IRequest<object>, ICacheableQuery
    {
        public string CacheKey => $"components:{ComponentType}:detail:{Id}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }
}
