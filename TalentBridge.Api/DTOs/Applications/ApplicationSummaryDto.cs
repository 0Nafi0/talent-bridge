namespace TalentBridge.Api.DTOs.Applications;

public record ApplicationSummaryDto(
    int ApplicationId,
    string JobTitle,
    string CompanyName,
    string Status,
    double MatchScore,
    DateTime AppliedAt
);