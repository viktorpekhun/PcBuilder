using Components.Application.Dtos;
using MediatR;
using PcBuilder.SharedKernel;
using PcBuilder.SharedKernel.Caching;
using PcBuilder.SharedKernel.Enums;

namespace Components.Application.Queries
{
    public record GetPriceHistoryQuery(
        ComponentType ComponentType,
        Guid Id,
        int Days) : IRequest<Result<List<PriceHistoryPointDto>>>, ICacheableQuery
    {
        public string CacheKey => $"components:{ComponentType}:price-history:{Id}:{Days}";
        public TimeSpan CacheDuration => TimeSpan.FromHours(1);
    }
}
