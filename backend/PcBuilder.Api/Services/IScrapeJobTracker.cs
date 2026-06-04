using PcBuilder.Api.Models;

namespace PcBuilder.Api.Services
{
    public interface IScrapeJobTracker
    {
        void TrackJob(ScrapeJobStatus status);
        ScrapeJobStatus? GetStatus(Guid jobId);
        IReadOnlyList<ScrapeJobStatus> GetAllStatuses();
        bool HasActiveJob(string componentType);
        void MarkRunning(Guid jobId);
        void MarkProgress(Guid jobId, int itemsScraped, int? totalItems);
        void MarkCompleted(Guid jobId, string? errorMessage = null, int? itemsScraped = null);
        void MarkCancelled(Guid jobId);
    }
}
