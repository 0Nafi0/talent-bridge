using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TalentBridge.Api.Data;
using TalentBridge.Api.DTOs.Applications;
using TalentBridge.Api.Models;

namespace TalentBridge.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ApplicationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────
    // POST /api/applications — Apply to a job [Candidate]
    // ────────────────────────────────────────────────────────────────
    [HttpPost]
    [Authorize(Policy = "CandidateOnly")]
    public async Task<IActionResult> Apply([FromBody] ApplyJobDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (userId is null) return Unauthorized();

        var candidate = await _db.CandidateProfiles
            .Include(c => c.CandidateSkills)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (candidate is null)
            return BadRequest(new { message = "Candidate profile not found." });

        var job = await _db.Jobs
            .Include(j => j.JobSkills)
            .FirstOrDefaultAsync(j => j.Id == dto.JobId);

        if (job is null || job.Status != JobStatus.Open)
            return BadRequest(new { message = "Job not available." });

        // Prevent duplicate application
        var alreadyApplied = await _db.JobApplications
            .AnyAsync(a => a.JobId == dto.JobId && a.CandidateProfileId == candidate.Id);

        if (alreadyApplied)
            return Conflict(new { message = "You have already applied to this job." });

        // Simple match score calculation
        var requiredSkills = job.JobSkills.Where(js => js.IsRequired).Select(js => js.SkillId);
        var candidateSkillIds = candidate.CandidateSkills.Select(cs => cs.SkillId);

        var matched = requiredSkills.Intersect(candidateSkillIds).Count();
        var totalRequired = requiredSkills.Count();
        double matchScore = totalRequired == 0 ? 100 : (matched * 100.0 / totalRequired);

        var application = new JobApplication
        {
            JobId = job.Id,
            CandidateProfileId = candidate.Id,
            Status = ApplicationStatus.Applied,
            AppliedAt = DateTime.UtcNow,
            MatchScore = matchScore
        };

        _db.JobApplications.Add(application);
        await _db.SaveChangesAsync();

        // Add timeline entry
        _db.ApplicationTimelines.Add(new ApplicationTimeline
        {
            ApplicationId = application.Id,
            Status = ApplicationStatus.Applied,
            ChangedByUserId = userId
        });

        await _db.SaveChangesAsync();

        return Ok(new { message = "Application submitted successfully.", application.Id });
    }

    // ────────────────────────────────────────────────────────────────
    // GET /api/applications/my — My applications [Candidate]
    // ────────────────────────────────────────────────────────────────
    [HttpGet("my")]
    [Authorize(Policy = "CandidateOnly")]
    public async Task<IActionResult> MyApplications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        var candidate = await _db.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (candidate is null) return Unauthorized();

        var applications = await _db.JobApplications
            .Include(a => a.Job)
                .ThenInclude(j => j.RecruiterProfile)
            .Where(a => a.CandidateProfileId == candidate.Id)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        var result = applications.Select(a => new ApplicationSummaryDto(
            ApplicationId: a.Id,
            JobTitle: a.Job.Title,
            CompanyName: a.Job.RecruiterProfile.CompanyName,
            Status: a.Status.ToString(),
            MatchScore: a.MatchScore,
            AppliedAt: a.AppliedAt
        ));

        return Ok(result);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /api/applications/{id} — Application detail
    // ────────────────────────────────────────────────────────────────
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetApplication(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        var application = await _db.JobApplications
            .Include(a => a.Job)
                .ThenInclude(j => j.RecruiterProfile)
            .Include(a => a.CandidateProfile)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null)
            return NotFound();

        // Candidate can see their own
        if (application.CandidateProfile.UserId != userId &&
            application.Job.RecruiterProfile.UserId != userId)
            return Forbid();

        return Ok(new ApplicationDetailDto(
            Id: application.Id,
            JobTitle: application.Job.Title,
            CompanyName: application.Job.RecruiterProfile.CompanyName,
            CandidateName: application.CandidateProfile.FullName,
            Status: application.Status.ToString(),
            MatchScore: application.MatchScore,
            AppliedAt: application.AppliedAt
        ));
    }

    // ────────────────────────────────────────────────────────────────
    // PUT /api/applications/{id}/status — Update status [Recruiter]
    // ────────────────────────────────────────────────────────────────
    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "RecruiterOnly")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateApplicationStatusDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        var application = await _db.JobApplications
            .Include(a => a.Job)
                .ThenInclude(j => j.RecruiterProfile)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null) return NotFound();

        // Ensure recruiter owns the job
        if (application.Job.RecruiterProfile.UserId != userId)
            return Forbid();

        if (!Enum.TryParse<ApplicationStatus>(dto.Status, true, out var newStatus))
            return BadRequest(new { message = "Invalid status value." });

        application.Status = newStatus;

        _db.ApplicationTimelines.Add(new ApplicationTimeline
        {
            ApplicationId = application.Id,
            Status = newStatus,
            Notes = dto.Notes,
            ChangedByUserId = userId!
        });

        await _db.SaveChangesAsync();

        return Ok(new { message = "Application status updated." });
    }

    // ────────────────────────────────────────────────────────────────
    // GET /api/applications/{id}/timeline — Application timeline
    // ────────────────────────────────────────────────────────────────
    [HttpGet("{id:int}/timeline")]
    [Authorize]
    public async Task<IActionResult> GetTimeline(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        var application = await _db.JobApplications
            .Include(a => a.Job)
                .ThenInclude(j => j.RecruiterProfile)
            .Include(a => a.CandidateProfile)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null) return NotFound();

        if (application.CandidateProfile.UserId != userId &&
            application.Job.RecruiterProfile.UserId != userId)
            return Forbid();

        var timeline = await _db.ApplicationTimelines
            .Where(t => t.ApplicationId == id)
            .OrderBy(t => t.ChangedAt)
            .Select(t => new ApplicationTimelineDto(
                Status: t.Status.ToString(),
                Notes: t.Notes,
                ChangedAt: t.ChangedAt
            ))
            .ToListAsync();

        return Ok(timeline);
    }
}