namespace TalentBridge.Api.Models;

public class CandidateSkill
{
    public int CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public int YearsOfExperience { get; set; }
}
