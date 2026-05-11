using Microsoft.EntityFrameworkCore;
using PcBuilder.Api.Models;
using PcBuilder.Persistence.Data;
using PcBuilder.Persistence.Data.Entities;
using System.Collections.Concurrent;

namespace PcBuilder.Api.Services
{
    public class DbScrapeJobTracker : IScrapeJobTracker
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DbScrapeJobTracker> _logger;
        private readonly ConcurrentDictionary<Guid, ScrapeJobStatus> _cache = new();
        private bool _hydrated;
        private readonly object _hydrateLock = new();

        public DbScrapeJobTracker(IServiceScopeFactory scopeFactory, ILogger<DbScrapeJobTracker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private void EnsureHydrated()
        {
            if (_hydrated) return;
            lock (_hydrateLock)
            {
                if (_hydrated) return;

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var jobs = db.ScrapeJobs
                    .AsNoTracking()
                    .OrderByDescending(j => j.QueuedAt)
                    .Take(500)
                    .ToList();

                foreach (var j in jobs)
                    _cache[j.Id] = ToStatus(j);

                _hydrated = true;
            }
        }

        public void TrackJob(ScrapeJobStatus status)
        {
            _cache[status.JobId] = status;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.ScrapeJobs.Add(new ScrapeJob
            {
                Id = status.JobId,
                ComponentType = status.ComponentType,
                Kind = status.Kind,
                State = status.State,
                QueuedAt = status.QueuedAt,
                StartedAt = status.StartedAt,
                CompletedAt = status.CompletedAt,
                ErrorMessage = status.ErrorMessage,
                ItemsScraped = status.ItemsScraped,
                TotalItems = status.TotalItems
            });
            db.SaveChanges();
        }

        public ScrapeJobStatus? GetStatus(Guid jobId)
        {
            EnsureHydrated();
            return _cache.TryGetValue(jobId, out var status) ? status : null;
        }

        public IReadOnlyList<ScrapeJobStatus> GetAllStatuses()
        {
            EnsureHydrated();
            return _cache.Values.OrderByDescending(j => j.QueuedAt).ToList();
        }

        public bool HasActiveJob(string componentType)
        {
            EnsureHydrated();
            return _cache.Values.Any(j =>
                j.ComponentType == componentType &&
                (j.State == "Queued" || j.State == "Running" || j.State == "Cancelling"));
        }

        public void MarkRunning(Guid jobId)
        {
            if (!_cache.TryGetValue(jobId, out var status)) return;

            status.State = "Running";
            status.StartedAt = DateTime.UtcNow;

            Persist(jobId, j =>
            {
                j.State = "Running";
                j.StartedAt = status.StartedAt;
            });
        }

        public void MarkCompleted(Guid jobId, string? errorMessage = null, int? itemsScraped = null)
        {
            if (!_cache.TryGetValue(jobId, out var status)) return;

            status.State = errorMessage == null ? "Completed" : "Failed";
            status.CompletedAt = DateTime.UtcNow;
            status.ErrorMessage = errorMessage;
            if (itemsScraped.HasValue)
                status.ItemsScraped = itemsScraped.Value;

            Persist(jobId, j =>
            {
                j.State = status.State;
                j.CompletedAt = status.CompletedAt;
                j.ErrorMessage = status.ErrorMessage;
                if (itemsScraped.HasValue)
                    j.ItemsScraped = itemsScraped.Value;
            });
        }

        public void MarkCancelled(Guid jobId)
        {
            if (!_cache.TryGetValue(jobId, out var status)) return;

            status.State = "Cancelling";

            Persist(jobId, j => j.State = "Cancelling");
        }

        private void Persist(Guid jobId, Action<ScrapeJob> mutate)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var entity = db.ScrapeJobs.FirstOrDefault(j => j.Id == jobId);
                if (entity == null) return;
                mutate(entity);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist scrape job {JobId}", jobId);
            }
        }

        private static ScrapeJobStatus ToStatus(ScrapeJob j) => new()
        {
            JobId = j.Id,
            ComponentType = j.ComponentType,
            Kind = j.Kind,
            State = j.State,
            QueuedAt = j.QueuedAt,
            StartedAt = j.StartedAt,
            CompletedAt = j.CompletedAt,
            ErrorMessage = j.ErrorMessage,
            ItemsScraped = j.ItemsScraped,
            TotalItems = j.TotalItems
        };
    }
}
