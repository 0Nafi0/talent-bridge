namespace TalentBridge.Api.Models;

public class CandidateProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? ResumeUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public int? YearsOfExperience { get; set; }

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = [];
    public ICollection<JobApplication> Applications { get; set; } = [];
}
