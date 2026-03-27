using MediatR;
using PcBuilder.SharedKernel.Caching;

namespace PcBuilder.SharedKernel.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly CacheService _cacheService;

        public CachingBehavior(CacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not ICacheableQuery cacheableQuery)
                return await next();

            if (_cacheService.TryGet<TResponse>(cacheableQuery.CacheKey, out var cached))
                return cached!;

            var response = await next();
            _cacheService.Set(cacheableQuery.CacheKey, response, cacheableQuery.CacheDuration);
            return response;
        }
    }
}
