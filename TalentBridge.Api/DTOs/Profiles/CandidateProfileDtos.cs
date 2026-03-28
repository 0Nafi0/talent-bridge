using TalentBridge.Api.DTOs.Jobs;

namespace TalentBridge.Api.DTOs.Profiles;

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record UpdateCandidateProfileDto(
    string? FullName,
    string? Headline,
    string? Bio,
    string? Location,
    string? ResumeUrl,
    string? LinkedInUrl,
    int? YearsOfExperience
);

public record AddCandidateSkillDto(
    int SkillId,
    int YearsOfExperience
);

// ── Response DTOs ────────────────────────────────────────────────────────────

public record CandidateSkillResponseDto(
    SkillDto Skill,
    int YearsOfExperience
);

public record CandidateProfileDto(
    int Id,
    string UserId,
    string FullName,
    string? Headline,
    string? Bio,
    string? Location,
    string? ResumeUrl,
    string? LinkedInUrl,
    int? YearsOfExperience,
    List<CandidateSkillResponseDto> Skills
);
