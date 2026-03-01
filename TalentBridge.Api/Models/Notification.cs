namespace TalentBridge.Api.Models;

public enum NotificationType
{
    StatusUpdate,
    NewApplication,
    General
}

public class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public NotificationType Type { get; set; } = NotificationType.General;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RelatedEntityId { get; set; } // e.g., applicationId or jobId
}
