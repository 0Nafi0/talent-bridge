using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TalentBridge.Api.Data;
using TalentBridge.Api.DTOs.Jobs;
using TalentBridge.Api.DTOs.Profiles;
using TalentBridge.Api.Models;

namespace TalentBridge.Api.Controllers;

[ApiController]
[Route("api/profile/candidate")]
[Authorize(Policy = "CandidateOnly")]
public class CandidateProfileController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public CandidateProfileController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.FindFirstValue("sub") 
            ?? string.Empty;
    }

    // ── GET /api/profile/candidate ──────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var profile = await _db.CandidateProfiles
            .Include(p => p.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null) return NotFound(new { message = "Candidate profile not found." });

        var dto = new CandidateProfileDto(
            Id: profile.Id,
            UserId: profile.UserId,
            FullName: profile.FullName,
            Headline: profile.Headline,
            Bio: profile.Bio,
            Location: profile.Location,
            ResumeUrl: profile.ResumeUrl,
            LinkedInUrl: profile.LinkedInUrl,
            YearsOfExperience: profile.YearsOfExperience,
            Skills: profile.CandidateSkills.Select(cs => new CandidateSkillResponseDto(
                Skill: new SkillDto(cs.Skill.Id, cs.Skill.Name, cs.Skill.Category),
                YearsOfExperience: cs.YearsOfExperience
            )).ToList()
        );

        return Ok(dto);
    }

    // ── PUT /api/profile/candidate ──────────────────────────────────────────
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateCandidateProfileDto dto)
    {
        var userId = GetUserId();
        var profile = await _db.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null) return NotFound(new { message = "Candidate profile not found." });

        if (dto.FullName != null) profile.FullName = dto.FullName;
        if (dto.Headline != null) profile.Headline = dto.Headline;
        if (dto.Bio != null) profile.Bio = dto.Bio;
        if (dto.Location != null) profile.Location = dto.Location;
        if (dto.ResumeUrl != null) profile.ResumeUrl = dto.ResumeUrl;
        if (dto.LinkedInUrl != null) profile.LinkedInUrl = dto.LinkedInUrl;
        if (dto.YearsOfExperience != null) profile.YearsOfExperience = dto.YearsOfExperience;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully." });
    }

    // ── POST /api/profile/candidate/skills ──────────────────────────────────
    [HttpPost("skills")]
    public async Task<IActionResult> AddSkill([FromBody] AddCandidateSkillDto dto)
    {
        if (dto.YearsOfExperience < 0) return BadRequest(new { message = "Years of experience cannot be negative." });

        var userId = GetUserId();
        var profile = await _db.CandidateProfiles
            .Include(p => p.CandidateSkills)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null) return NotFound(new { message = "Candidate profile not found." });

        var skill = await _db.Skills.FindAsync(dto.SkillId);
        if (skill is null) return NotFound(new { message = $"Skill with ID {dto.SkillId} not found." });

        var existingSkill = profile.CandidateSkills.FirstOrDefault(cs => cs.SkillId == dto.SkillId);
        if (existingSkill != null)
        {
            // Update years of experience if it already exists
            existingSkill.YearsOfExperience = dto.YearsOfExperience;
        }
        else
        {
            profile.CandidateSkills.Add(new CandidateSkill
            {
                CandidateProfileId = profile.Id,
                SkillId = dto.SkillId,
                YearsOfExperience = dto.YearsOfExperience
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = $"Skill '{skill.Name}' added successfully to your profile." });
    }

    // ── DELETE /api/profile/candidate/skills/{skillId} ──────────────────────
    [HttpDelete("skills/{skillId:int}")]
    public async Task<IActionResult> RemoveSkill(int skillId)
    {
        var userId = GetUserId();
        var profile = await _db.CandidateProfiles
            .Include(p => p.CandidateSkills)
                .ThenInclude(cs => cs.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null) return NotFound(new { message = "Candidate profile not found." });

        var existingSkill = profile.CandidateSkills.FirstOrDefault(cs => cs.SkillId == skillId);
        if (existingSkill is null) return NotFound(new { message = "Skill not found in your profile." });

        var skillName = existingSkill.Skill.Name;
        profile.CandidateSkills.Remove(existingSkill);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Skill '{skillName}' removed successfully from your profile." });
    }
}
