namespace TalentBridge.Api.DTOs.Applications;

public record ApplicationDetailDto(
    int Id,
    string JobTitle,
    string CompanyName,
    string CandidateName,
    string Status,
    double MatchScore,
    DateTime AppliedAt
);