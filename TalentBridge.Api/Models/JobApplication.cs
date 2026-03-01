namespace TalentBridge.Api.Models;

public enum ApplicationStatus
{
    Applied,
    Shortlisted,
    Interview,
    Offered,
    Rejected,
    Withdrawn
}

public class JobApplication
{
    public int Id { get; set; }

    public int CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public string? CoverLetter { get; set; }
    public double MatchScore { get; set; } // 0–100 calculated at application time
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationTimeline> Timelines { get; set; } = [];
}
