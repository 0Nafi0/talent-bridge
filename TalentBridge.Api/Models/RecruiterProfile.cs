namespace TalentBridge.Api.Models;

public class RecruiterProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string? Location { get; set; }

    public ICollection<Job> Jobs { get; set; } = [];
}
