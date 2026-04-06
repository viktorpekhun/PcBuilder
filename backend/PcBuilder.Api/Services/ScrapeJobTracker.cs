using PcBuilder.Api.Models;
using System.Collections.Concurrent;

namespace PcBuilder.Api.Services
{
    public class ScrapeJobTracker : IScrapeJobTracker
    {
        private readonly ConcurrentDictionary<Guid, ScrapeJobStatus> _jobs = new();

        public void TrackJob(ScrapeJobStatus status)
        {
            _jobs[status.JobId] = status;
        }

        public ScrapeJobStatus? GetStatus(Guid jobId)
        {
            return _jobs.TryGetValue(jobId, out var status) ? status : null;
        }

        public IReadOnlyList<ScrapeJobStatus> GetAllStatuses()
        {
            return _jobs.Values.OrderByDescending(j => j.QueuedAt).ToList();
        }

        public bool HasActiveJob(string componentType)
        {
            return _jobs.Values.Any(j =>
                j.ComponentType == componentType &&
                (j.State == "Queued" || j.State == "Running"));
        }

        public void MarkCompleted(Guid jobId, string? errorMessage = null)
        {
            if (_jobs.TryGetValue(jobId, out var status))
            {
                status.State = errorMessage == null ? "Completed" : "Failed";
                status.CompletedAt = DateTime.UtcNow;
                status.ErrorMessage = errorMessage;
            }
        }

        public void MarkCancelled(Guid jobId)
        {
            if (_jobs.TryGetValue(jobId, out var status))
            {
                status.State = "Cancelling";
            }
        }
    }
}
