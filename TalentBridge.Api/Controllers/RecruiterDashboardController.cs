using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TalentBridge.Api.Data;
using TalentBridge.Api.DTOs.Jobs;
using TalentBridge.Api.DTOs.Profiles;

namespace TalentBridge.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "RecruiterOnly")]
public class RecruiterDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public RecruiterDashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.FindFirstValue("sub") 
            ?? string.Empty;
    }

    private async Task<int?> GetRecruiterProfileIdAsync()
    {
        var userId = GetUserId();
        var profile = await _db.RecruiterProfiles.FirstOrDefaultAsync(rp => rp.UserId == userId);
        return profile?.Id;
    }

    // ── GET /api/dashboard/jobs ─────────────────────────────────────────────
    [HttpGet("jobs")]
    public async Task<IActionResult> GetMyJobs()
    {
        var profileId = await GetRecruiterProfileIdAsync();
        if (profileId is null) return Unauthorized(new { message = "Recruiter profile not found." });

        var jobs = await _db.Jobs
            .Include(j => j.JobSkills)
                .ThenInclude(js => js.Skill)
            .Include(j => j.Applications)
            .Where(j => j.RecruiterProfileId == profileId.Value)
            .OrderByDescending(j => j.PostedAt)
            .ToListAsync();

        var dtos = jobs.Select(j => new JobSummaryDto(
            Id: j.Id,
            Title: j.Title,
            CompanyName: j.RecruiterProfile.CompanyName, // Can fetch directly if we include, otherwise we know who it is.
            Location: j.Location,
            JobType: j.JobType.ToString(),
            Status: j.Status.ToString(),
            SalaryMin: j.SalaryMin,
            SalaryMax: j.SalaryMax,
            ExperienceLevel: j.ExperienceLevel,
            PostedAt: j.PostedAt,
            ApplicationCount: j.Applications.Count,
            RequiredSkills: j.JobSkills
                .Where(js => js.IsRequired)
                .Select(js => new SkillDto(js.Skill.Id, js.Skill.Name, js.Skill.Category))
                .ToList()
        )).ToList();

        // Need to properly set CompanyName by including it or using the db directly
        // Let's reload jobs with RecruiterProfile explicitly
        var jobsWithProfile = await _db.Jobs
            .Include(j => j.RecruiterProfile)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(j => j.Applications)
            .Where(j => j.RecruiterProfileId == profileId.Value)
            .OrderByDescending(j => j.PostedAt)
            .ToListAsync();

        var actualDtos = jobsWithProfile.Select(j => new JobSummaryDto(
            Id: j.Id,
            Title: j.Title,
            CompanyName: j.RecruiterProfile.CompanyName,
            Location: j.Location,
            JobType: j.JobType.ToString(),
            Status: j.Status.ToString(),
            SalaryMin: j.SalaryMin,
            SalaryMax: j.SalaryMax,
            ExperienceLevel: j.ExperienceLevel,
            PostedAt: j.PostedAt,
            ApplicationCount: j.Applications.Count,
            RequiredSkills: j.JobSkills
                .Where(js => js.IsRequired)
                .Select(js => new SkillDto(js.Skill.Id, js.Skill.Name, js.Skill.Category))
                .ToList()
        )).ToList();

        return Ok(actualDtos);
    }

    // ── GET /api/dashboard/applicants ────────────────────────────────────────
    [HttpGet("applicants")]
    public async Task<IActionResult> GetAllApplicants([FromQuery] DashboardApplicantFilterDto filter)
    {
        var profileId = await GetRecruiterProfileIdAsync();
        if (profileId is null) return Unauthorized(new { message = "Recruiter profile not found." });

        var query = _db.JobApplications
            .Include(a => a.CandidateProfile)
                .ThenInclude(cp => cp.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
            .Include(a => a.Job)
            .Where(a => a.Job.RecruiterProfileId == profileId.Value)
            .AsQueryable();

        // Filter by skills
        if (filter.SkillIds is not null && filter.SkillIds.Count > 0)
        {
            foreach (var sid in filter.SkillIds)
            {
                query = query.Where(a => a.CandidateProfile.CandidateSkills.Any(cs => cs.SkillId == sid));
            }
        }

        // Filter by min experience
        if (filter.MinExperience.HasValue)
        {
            query = query.Where(a => a.CandidateProfile.YearsOfExperience >= filter.MinExperience.Value);
        }

        var applications = await query
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        var applicants = applications.Select(a => new ApplicantDto(
            ApplicationId: a.Id,
            CandidateFullName: a.CandidateProfile.FullName,
            Headline: a.CandidateProfile.Headline,
            Location: a.CandidateProfile.Location,
            YearsOfExperience: a.CandidateProfile.YearsOfExperience,
            MatchScore: a.MatchScore,
            Status: a.Status.ToString(),
            AppliedAt: a.AppliedAt,
            Skills: a.CandidateProfile.CandidateSkills.Select(cs => 
                new SkillDto(cs.Skill.Id, cs.Skill.Name, cs.Skill.Category)).ToList()
        )).ToList();

        return Ok(applicants);
    }

    // ── GET /api/dashboard/profile ──────────────────────────────────────────
    [HttpGet("profile")]
    public async Task<IActionResult> GetRecruiterProfile()
    {
        var userId = GetUserId();
        var profile = await _db.RecruiterProfiles
            .Include(p => p.Jobs)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null) return NotFound(new { message = "Recruiter profile not found." });

        var dto = new RecruiterProfileDto(
            Id: profile.Id,
            UserId: profile.UserId,
            FullName: profile.FullName,
            CompanyName: profile.CompanyName,
            CompanyDescription: profile.CompanyDescription,
            CompanyWebsite: profile.CompanyWebsite,
            CompanyLogoUrl: profile.CompanyLogoUrl,
            Location: profile.Location,
            TotalJobsPosted: profile.Jobs.Count
        );

        return Ok(dto);
    }

    // ── PUT /api/dashboard/profile ──────────────────────────────────────────
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateRecruiterProfile([FromBody] UpdateRecruiterProfileDto dto)
    {
        var userId = GetUserId();
        var profile = await _db.RecruiterProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null) return NotFound(new { message = "Recruiter profile not found." });

        if (dto.FullName != null) profile.FullName = dto.FullName;
        if (dto.CompanyName != null) profile.CompanyName = dto.CompanyName;
        if (dto.CompanyDescription != null) profile.CompanyDescription = dto.CompanyDescription;
        if (dto.CompanyWebsite != null) profile.CompanyWebsite = dto.CompanyWebsite;
        if (dto.CompanyLogoUrl != null) profile.CompanyLogoUrl = dto.CompanyLogoUrl;
        if (dto.Location != null) profile.Location = dto.Location;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Recruiter profile updated successfully." });
    }
}
