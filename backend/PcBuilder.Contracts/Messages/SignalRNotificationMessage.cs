namespace PcBuilder.Contracts.Messages
{
    public record SignalRNotificationMessage(
        Guid NotificationId,
        Guid UserId,
        string Type,
        string Payload,
        DateTime CreatedAt
    );
}
