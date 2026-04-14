namespace Moderation.Application.Dtos
{
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersLast7Days { get; set; }
        public int TotalBuilds { get; set; }
        public int PublishedBuilds { get; set; }
        public int TotalReviews { get; set; }
        public int PendingReports { get; set; }
        public Dictionary<string, int> ComponentCounts { get; set; } = new();
        public int ActiveBannedUsers { get; set; }
    }
}
