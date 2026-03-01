using TalentBridge.Api.Models;

namespace TalentBridge.Api.DTOs.Jobs;

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateJobDto(
    string Title,
    string Description,
    string Location,
    string JobType,          // "FullTime" | "PartTime" | "Contract" | "Remote" | "Internship"
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? ExperienceLevel, // "Junior" | "Mid" | "Senior"
    DateTime? ExpiresAt,
    List<int> RequiredSkillIds,
    List<int> OptionalSkillIds
);

public record UpdateJobDto(
    string? Title,
    string? Description,
    string? Location,
    string? JobType,
    string? Status,          // "Open" | "Closed" | "Draft"
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? ExperienceLevel,
    DateTime? ExpiresAt,
    List<int>? RequiredSkillIds,
    List<int>? OptionalSkillIds
);

// Query parameters for GET /api/jobs
public class JobFilterDto
{
    public string? Location      { get; set; }
    public string? JobType       { get; set; }
    public string? ExperienceLevel { get; set; }
    public string? Keyword       { get; set; }  // searches title + description
    public decimal? SalaryMin    { get; set; }
    public decimal? SalaryMax    { get; set; }
    public List<int>? SkillIds   { get; set; }  // must have ALL of these skills
    public int Page              { get; set; } = 1;
    public int PageSize          { get; set; } = 10;
}

// ── Response DTOs ────────────────────────────────────────────────────────────

public record SkillDto(int Id, string Name, string Category);

public record JobSkillDto(SkillDto Skill, bool IsRequired);

public record JobSummaryDto(
    int Id,
    string Title,
    string CompanyName,
    string Location,
    string JobType,
    string Status,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? ExperienceLevel,
    DateTime PostedAt,
    int ApplicationCount,
    List<SkillDto> RequiredSkills
);

public record JobDetailDto(
    int Id,
    string Title,
    string Description,
    string CompanyName,
    string? CompanyWebsite,
    string Location,
    string JobType,
    string Status,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? ExperienceLevel,
    DateTime PostedAt,
    DateTime? ExpiresAt,
    int ApplicationCount,
    List<JobSkillDto> Skills
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// ── Applicant summary (for GET /api/jobs/{id}/applicants) ────────────────────

public record ApplicantDto(
    int ApplicationId,
    string CandidateFullName,
    string? Headline,
    string? Location,
    int? YearsOfExperience,
    double MatchScore,
    string Status,
    DateTime AppliedAt,
    List<SkillDto> Skills
);
