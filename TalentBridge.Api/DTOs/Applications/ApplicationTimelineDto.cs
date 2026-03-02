namespace TalentBridge.Api.DTOs.Applications;

public record ApplicationTimelineDto(
    string Status,
    string? Notes,
    DateTime ChangedAt
);