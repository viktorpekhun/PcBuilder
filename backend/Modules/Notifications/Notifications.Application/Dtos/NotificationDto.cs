namespace Notifications.Application.Dtos
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, string> Payload { get; set; } = new();
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
