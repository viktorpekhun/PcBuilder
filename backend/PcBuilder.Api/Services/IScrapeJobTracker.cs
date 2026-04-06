using PcBuilder.Api.Models;

namespace PcBuilder.Api.Services
{
    public interface IScrapeJobTracker
    {
        void TrackJob(ScrapeJobStatus status);
        ScrapeJobStatus? GetStatus(Guid jobId);
        IReadOnlyList<ScrapeJobStatus> GetAllStatuses();
        bool HasActiveJob(string componentType);
        void MarkCompleted(Guid jobId, string? errorMessage = null);
        void MarkCancelled(Guid jobId);
    }
}
