namespace PcBuilder.SharedKernel.Services
{
    public interface IProfanityFilterService
    {
        bool ContainsProfanity(string text);
    }
}