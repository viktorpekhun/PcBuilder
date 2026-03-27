namespace PcBuilder.SharedKernel.Caching
{
    public interface ICacheInvalidator
    {
        void InvalidateByPrefix(string prefix);
    }
}
