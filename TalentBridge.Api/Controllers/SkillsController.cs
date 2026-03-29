using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentBridge.Api.Data;
using TalentBridge.Api.DTOs.Jobs; // Reusing SkillDto from Jobs DTOs

namespace TalentBridge.Api.Controllers;

[ApiController]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SkillsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── GET /api/skills ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetSkills([FromQuery] string? category)
    {
        var query = _db.Skills.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(s => s.Category.ToLower() == category.ToLower());
        }

        var skills = await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();

        var dtos = skills.Select(s => new SkillDto(s.Id, s.Name, s.Category)).ToList();

        return Ok(dtos);
    }
}
