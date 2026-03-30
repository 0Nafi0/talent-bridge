using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TalentBridge.Api.Data;
using TalentBridge.Api.DTOs.Analytics;
using TalentBridge.Api.Models;

namespace TalentBridge.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AnalyticsController(ApplicationDbContext db)
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

    // ── GET /api/analytics/overview ──────────────────────────────────────────
    [HttpGet("overview")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetPlatformOverview()
    {
        var totalJobs = await _db.Jobs.CountAsync();
        var activeJobs = await _db.Jobs.CountAsync(j => j.Status == JobStatus.Open);
        var totalCandidates = await _db.CandidateProfiles.CountAsync();
        var totalRecruiters = await _db.RecruiterProfiles.CountAsync();
        var totalApplications = await _db.JobApplications.CountAsync();

        var dto = new PlatformOverviewDto(
            TotalJobs: totalJobs,
            ActiveJobs: activeJobs,
            TotalCandidates: totalCandidates,
            TotalRecruiters: totalRecruiters,
            TotalApplications: totalApplications
        );

        return Ok(dto);
    }

    // ── GET /api/analytics/jobs/{id} ────────────────────────────────────────
    [HttpGet("jobs/{id:int}")]
    [Authorize(Policy = "RecruiterOnly")]
    public async Task<IActionResult> GetJobAnalytics(int id)
    {
        var profileId = await GetRecruiterProfileIdAsync();
        if (profileId is null) return Unauthorized(new { message = "Recruiter profile not found." });

        var job = await _db.Jobs
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == id && j.RecruiterProfileId == profileId.Value);

        if (job is null) return NotFound(new { message = "Job not found or you do not have permission to view its analytics." });

        var apps = job.Applications;
        var avgMatchScore = apps.Any() ? apps.Average(a => a.MatchScore) : 0;

        var dto = new JobAnalyticsDto(
            JobId: job.Id,
            JobTitle: job.Title,
            TotalApplications: apps.Count,
            Shortlisted: apps.Count(a => a.Status == ApplicationStatus.Shortlisted || a.Status == ApplicationStatus.Interview),
            Rejected: apps.Count(a => a.Status == ApplicationStatus.Rejected),
            Offered: apps.Count(a => a.Status == ApplicationStatus.Offered),
            AverageMatchScore: avgMatchScore
        );

        return Ok(dto);
    }

    // ── GET /api/analytics/recruiter ────────────────────────────────────────
    [HttpGet("recruiter")]
    [Authorize(Policy = "RecruiterOnly")]
    public async Task<IActionResult> GetRecruiterAnalytics()
    {
        var profileId = await GetRecruiterProfileIdAsync();
        if (profileId is null) return Unauthorized(new { message = "Recruiter profile not found." });

        var jobs = await _db.Jobs
            .Include(j => j.Applications)
            .Where(j => j.RecruiterProfileId == profileId.Value)
            .ToListAsync();

        var allApps = jobs.SelectMany(j => j.Applications).ToList();

        var dto = new RecruiterAnalyticsDto(
            TotalJobsPosted: jobs.Count,
            ActiveJobs: jobs.Count(j => j.Status == JobStatus.Open),
            TotalApplicationsReceived: allApps.Count,
            ShortlistedCandidates: allApps.Count(a => a.Status == ApplicationStatus.Shortlisted || a.Status == ApplicationStatus.Interview),
            OfferedCandidates: allApps.Count(a => a.Status == ApplicationStatus.Offered)
        );

        return Ok(dto);
    }
}
