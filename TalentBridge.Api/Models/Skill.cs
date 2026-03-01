namespace TalentBridge.Api.Models;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g. Programming, Design, Management

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = [];
    public ICollection<JobSkill> JobSkills { get; set; } = [];
}
