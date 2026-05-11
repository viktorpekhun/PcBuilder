namespace Notifications.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Payload { get; set; } = "{}";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
