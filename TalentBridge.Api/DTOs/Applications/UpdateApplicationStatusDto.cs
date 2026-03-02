namespace TalentBridge.Api.DTOs.Applications;

public record UpdateApplicationStatusDto(
    string Status,
    string? Notes
);