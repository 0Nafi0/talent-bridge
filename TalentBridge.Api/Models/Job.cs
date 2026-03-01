namespace TalentBridge.Api.Models;

public enum JobType
{
    FullTime,
    PartTime,
    Contract,
    Remote,
    Internship
}

public enum JobStatus
{
    Open,
    Closed,
    Draft
}

public class Job
{
    public int Id { get; set; }

    public int RecruiterProfileId { get; set; }
    public RecruiterProfile RecruiterProfile { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public JobType JobType { get; set; } = JobType.FullTime;
    public JobStatus Status { get; set; } = JobStatus.Open;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public string? ExperienceLevel { get; set; } // Junior, Mid, Senior
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public ICollection<JobSkill> JobSkills { get; set; } = [];
    public ICollection<JobApplication> Applications { get; set; } = [];
}
