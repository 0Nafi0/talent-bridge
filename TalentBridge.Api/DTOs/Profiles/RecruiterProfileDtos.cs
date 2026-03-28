namespace TalentBridge.Api.DTOs.Profiles;

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record UpdateRecruiterProfileDto(
    string? FullName,
    string? CompanyName,
    string? CompanyDescription,
    string? CompanyWebsite,
    string? CompanyLogoUrl,
    string? Location
);

// ── Response DTOs ────────────────────────────────────────────────────────────

public record RecruiterProfileDto(
    int Id,
    string UserId,
    string FullName,
    string CompanyName,
    string? CompanyDescription,
    string? CompanyWebsite,
    string? CompanyLogoUrl,
    string? Location,
    int TotalJobsPosted
);

// ── Query Params for Applicants ──────────────────────────────────────────────

public class DashboardApplicantFilterDto
{
    public List<int>? SkillIds { get; set; }
    public int? MinExperience { get; set; }
}
