using Components.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Queries
{
    public record GetComponentByIdQuery(
        Guid Id,
        ComponentType ComponentType) : IRequest<Result<IComponentDetailDto>>, ICacheableQuery
    {
        public string CacheKey => $"components:{ComponentType}:detail:{Id}";
        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }
}
