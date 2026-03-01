using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TalentBridge.Api.Data;
using TalentBridge.Api.DTOs.Jobs;
using TalentBridge.Api.Models;

namespace TalentBridge.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public JobsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── GET /api/jobs ──────────────────────────────────────────────────────────
    // Public: list open jobs with pagination + multi-param filtering
    [HttpGet]
    public async Task<IActionResult> GetJobs([FromQuery] JobFilterDto filter)
    {
        var query = _db.Jobs
            .Include(j => j.RecruiterProfile)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(j => j.Applications)
            .Where(j => j.Status == JobStatus.Open)
            .AsQueryable();

        // ── Filters ──────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var kw = filter.Keyword.ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(kw) ||
                j.Description.ToLower().Contains(kw) ||
                j.RecruiterProfile.CompanyName.ToLower().Contains(kw));
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
            query = query.Where(j => j.Location.ToLower().Contains(filter.Location.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.ExperienceLevel))
            query = query.Where(j => j.ExperienceLevel != null &&
                j.ExperienceLevel.ToLower() == filter.ExperienceLevel.ToLower());

        if (!string.IsNullOrWhiteSpace(filter.JobType) &&
            Enum.TryParse<JobType>(filter.JobType, true, out var jobType))
            query = query.Where(j => j.JobType == jobType);

        if (filter.SalaryMin.HasValue)
            query = query.Where(j => j.SalaryMax == null || j.SalaryMax >= filter.SalaryMin);

        if (filter.SalaryMax.HasValue)
            query = query.Where(j => j.SalaryMin == null || j.SalaryMin <= filter.SalaryMax);

        // Skill filter: job must have ALL of the requested skill IDs
        if (filter.SkillIds is { Count: > 0 })
        {
            foreach (var skillId in filter.SkillIds)
                query = query.Where(j => j.JobSkills.Any(js => js.SkillId == skillId));
        }

        // ── Pagination ───────────────────────────────────────────────────────
        var pageSize  = Math.Clamp(filter.PageSize, 1, 50);
        var page      = Math.Max(filter.Page, 1);
        var total     = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        var jobs = await query
            .OrderByDescending(j => j.PostedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = jobs.Select(j => new JobSummaryDto(
            Id:               j.Id,
            Title:            j.Title,
            CompanyName:      j.RecruiterProfile.CompanyName,
            Location:         j.Location,
            JobType:          j.JobType.ToString(),
            Status:           j.Status.ToString(),
            SalaryMin:        j.SalaryMin,
            SalaryMax:        j.SalaryMax,
            ExperienceLevel:  j.ExperienceLevel,
            PostedAt:         j.PostedAt,
            ApplicationCount: j.Applications.Count,
            RequiredSkills:   j.JobSkills
                .Where(js => js.IsRequired)
                .Select(js => new SkillDto(js.Skill.Id, js.Skill.Name, js.Skill.Category))
                .ToList()
        ));

        return Ok(new PagedResult<JobSummaryDto>(items, total, page, pageSize, totalPages));
    }

    // ── GET /api/jobs/{id} ────────────────────────────────────────────────────
    // Public: full job detail
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var job = await _db.Jobs
            .Include(j => j.RecruiterProfile)
            .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null) return NotFound(new { message = $"Job {id} not found." });

        return Ok(new JobDetailDto(
            Id:               job.Id,
            Title:            job.Title,
            Description:      job.Description,
            CompanyName:      job.RecruiterProfile.CompanyName,
            CompanyWebsite:   job.RecruiterProfile.CompanyWebsite,
            Location:         job.Location,
            JobType:          job.JobType.ToString(),
            Status:           job.Status.ToString(),
            SalaryMin:        job.SalaryMin,
            SalaryMax:        job.SalaryMax,
            ExperienceLevel:  job.ExperienceLevel,
            PostedAt:         job.PostedAt,
            ExpiresAt:        job.ExpiresAt,
            ApplicationCount: job.Applications.Count,
            Skills:           job.JobSkills.Select(js => new JobSkillDto(
                                   new SkillDto(js.Skill.Id, js.Skill.Name, js.Skill.Category),
                                   js.IsRequired)).ToList()
        ));
    }

    // ── POST /api/jobs ────────────────────────────────────────────────────────
    // Recruiter only: create a job
    [HttpPost]
    [Authorize(Policy = "RecruiterOnly")]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (userId is null) return Unauthorized();

        var recruiterProfile = await _db.RecruiterProfiles
            .FirstOrDefaultAsync(r => r.UserId == userId);
        if (recruiterProfile is null)
            return BadRequest(new { message = "Recruiter profile not found. Please complete your profile first." });

        if (!Enum.TryParse<JobType>(dto.JobType, true, out var jobType))
            return BadRequest(new { message = $"Invalid JobType '{dto.JobType}'. Valid values: FullTime, PartTime, Contract, Remote, Internship." });

        // Validate all skill IDs exist
        var allSkillIds = dto.RequiredSkillIds.Concat(dto.OptionalSkillIds).Distinct();
        var existingSkillIds = await _db.Skills
            .Where(s => allSkillIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();
        var missing = allSkillIds.Except(existingSkillIds).ToList();
        if (missing.Count > 0)
            return BadRequest(new { message = $"Skill IDs not found: {string.Join(", ", missing)}" });

        var job = new Job
        {
            RecruiterProfileId = recruiterProfile.Id,
            Title              = dto.Title,
            Description        = dto.Description,
            Location           = dto.Location,
            JobType            = jobType,
            Status             = JobStatus.Open,
            SalaryMin          = dto.SalaryMin,
            SalaryMax          = dto.SalaryMax,
            ExperienceLevel    = dto.ExperienceLevel,
            PostedAt           = DateTime.UtcNow,
            ExpiresAt          = dto.ExpiresAt,
            JobSkills          =
                dto.RequiredSkillIds.Select(id => new JobSkill { SkillId = id, IsRequired = true }).Concat(
                dto.OptionalSkillIds.Select(id => new JobSkill { SkillId = id, IsRequired = false }))
                .ToList()
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, new { job.Id, job.Title, job.Status });
    }

    // ── PUT /api/jobs/{id} ────────────────────────────────────────────────────
    // Recruiter only: update their own job
    [HttpPut("{id:int}")]
    [Authorize(Policy = "RecruiterOnly")]
    public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (userId is null) return Unauthorized();

        var job = await _db.Jobs
            .Include(j => j.RecruiterProfile)
            .Include(j => j.JobSkills)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null) return NotFound(new { message = $"Job {id} not found." });

        // Ensure the recruiter owns this job
        if (job.RecruiterProfile.UserId != userId)
            return Forbid();

        if (dto.Title       is not null) job.Title           = dto.Title;
        if (dto.Description is not null) job.Description     = dto.Description;
        if (dto.Location    is not null) job.Location        = dto.Location;
        if (dto.SalaryMin   is not null) job.SalaryMin       = dto.SalaryMin;
        if (dto.SalaryMax   is not null) job.SalaryMax       = dto.SalaryMax;
        if (dto.ExperienceLevel is not null) job.ExperienceLevel = dto.ExperienceLevel;
        if (dto.ExpiresAt   is not null) job.ExpiresAt       = dto.ExpiresAt;

        if (dto.JobType is not null)
        {
            if (!Enum.TryParse<JobType>(dto.JobType, true, out var jobType))
                return BadRequest(new { message = $"Invalid JobType '{dto.JobType}'." });
            job.JobType = jobType;
        }

        if (dto.Status is not null)
        {
            if (!Enum.TryParse<JobStatus>(dto.Status, true, out var status))
                return BadRequest(new { message = $"Invalid Status '{dto.Status}'." });
            job.Status = status;
        }

        // Replace skills if provided
        if (dto.RequiredSkillIds is not null || dto.OptionalSkillIds is not null)
        {
            var required = dto.RequiredSkillIds ?? [];
            var optional = dto.OptionalSkillIds ?? [];
            var allIds   = required.Concat(optional).Distinct();

            var existingIds = await _db.Skills
                .Where(s => allIds.Contains(s.Id))
                .Select(s => s.Id).ToListAsync();
            var missing = allIds.Except(existingIds).ToList();
            if (missing.Count > 0)
                return BadRequest(new { message = $"Skill IDs not found: {string.Join(", ", missing)}" });

            _db.JobSkills.RemoveRange(job.JobSkills);
            job.JobSkills = required.Select(sid => new JobSkill { JobId = job.Id, SkillId = sid, IsRequired = true })
                .Concat(optional.Select(sid => new JobSkill { JobId = job.Id, SkillId = sid, IsRequired = false }))
                .ToList();
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Job updated successfully.", job.Id });
    }

    // ── DELETE /api/jobs/{id} ─────────────────────────────────────────────────
    // Recruiter only: delete their own job
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "RecruiterOnly")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (userId is null) return Unauthorized();

        var job = await _db.Jobs
            .Include(j => j.RecruiterProfile)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null) return NotFound(new { message = $"Job {id} not found." });
        if (job.RecruiterProfile.UserId != userId) return Forbid();

        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Job '{job.Title}' deleted successfully." });
    }

    // ── GET /api/jobs/{id}/applicants ─────────────────────────────────────────
    // Recruiter only: list applicants for their job (sorted by match score)
    [HttpGet("{id:int}/applicants")]
    [Authorize(Policy = "RecruiterOnly")]
    public async Task<IActionResult> GetApplicants(
        int id,
        [FromQuery] string? status,
        [FromQuery] int? minExperience,
        [FromQuery] List<int>? skillIds,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (userId is null) return Unauthorized();

        var job = await _db.Jobs
            .Include(j => j.RecruiterProfile)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job is null) return NotFound(new { message = $"Job {id} not found." });
        if (job.RecruiterProfile.UserId != userId) return Forbid();

        var query = _db.JobApplications
            .Include(a => a.CandidateProfile)
                .ThenInclude(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
            .Where(a => a.JobId == id)
            .AsQueryable();

        // ── Optional filters ─────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<ApplicationStatus>(status, true, out var appStatus))
            query = query.Where(a => a.Status == appStatus);

        if (minExperience.HasValue)
            query = query.Where(a =>
                a.CandidateProfile.YearsOfExperience == null ||
                a.CandidateProfile.YearsOfExperience >= minExperience);

        if (skillIds is { Count: > 0 })
            foreach (var sid in skillIds)
                query = query.Where(a =>
                    a.CandidateProfile.CandidateSkills.Any(cs => cs.SkillId == sid));

        var total     = await query.CountAsync();
        var clamped   = Math.Clamp(pageSize, 1, 100);
        var totalPages = (int)Math.Ceiling(total / (double)clamped);

        var applications = await query
            .OrderByDescending(a => a.MatchScore)
            .ThenBy(a => a.AppliedAt)
            .Skip((Math.Max(page, 1) - 1) * clamped)
            .Take(clamped)
            .ToListAsync();

        var result = applications.Select(a => new ApplicantDto(
            ApplicationId:    a.Id,
            CandidateFullName: a.CandidateProfile.FullName,
            Headline:         a.CandidateProfile.Headline,
            Location:         a.CandidateProfile.Location,
            YearsOfExperience: a.CandidateProfile.YearsOfExperience,
            MatchScore:       a.MatchScore,
            Status:           a.Status.ToString(),
            AppliedAt:        a.AppliedAt,
            Skills:           a.CandidateProfile.CandidateSkills
                .Select(cs => new SkillDto(cs.Skill.Id, cs.Skill.Name, cs.Skill.Category))
                .ToList()
        ));

        return Ok(new PagedResult<ApplicantDto>(result, total, page, clamped, totalPages));
    }
}
