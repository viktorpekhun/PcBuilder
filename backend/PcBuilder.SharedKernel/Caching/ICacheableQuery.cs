namespace PcBuilder.SharedKernel.Caching
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        TimeSpan CacheDuration { get; }
    }
}
