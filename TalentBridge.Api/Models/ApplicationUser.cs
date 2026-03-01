using Microsoft.AspNetCore.Identity;

namespace TalentBridge.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Candidate"; // Candidate | Recruiter | Admin

    public CandidateProfile? CandidateProfile { get; set; }
    public RecruiterProfile? RecruiterProfile { get; set; }
    public ICollection<Notification> Notifications { get; set; } = [];
}
