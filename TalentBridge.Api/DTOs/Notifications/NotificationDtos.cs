namespace TalentBridge.Api.DTOs.Notifications;

public record NotificationDto(
    int Id,
    string Title,
    string Message,
    bool IsRead,
    string Type,
    DateTime CreatedAt,
    string? RelatedEntityId
);
