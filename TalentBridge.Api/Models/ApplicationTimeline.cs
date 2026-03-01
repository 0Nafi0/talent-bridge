namespace TalentBridge.Api.Models;

public class ApplicationTimeline
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public JobApplication Application { get; set; } = null!;

    public ApplicationStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedByUserId { get; set; } = string.Empty;
}
