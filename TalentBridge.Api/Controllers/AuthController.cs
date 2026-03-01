using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TalentBridge.Api.Data;
using TalentBridge.Api.DTOs.Auth;
using TalentBridge.Api.Models;
using TalentBridge.Api.Services;

namespace TalentBridge.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _db;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _db = db;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Only allow Candidate or Recruiter during self-registration
        var allowedRoles = new[] { "Candidate", "Recruiter" };
        if (!allowedRoles.Contains(dto.Role))
            return BadRequest(new { message = "Role must be 'Candidate' or 'Recruiter'." });

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            return Conflict(new { message = "A user with this email already exists." });

        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            Role = dto.Role
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        // Assign role
        await _userManager.AddToRoleAsync(user, dto.Role);

        // Create profile stub
        if (dto.Role == "Candidate")
        {
            _db.CandidateProfiles.Add(new CandidateProfile
            {
                UserId = user.Id,
                FullName = dto.FullName
            });
        }
        else
        {
            _db.RecruiterProfiles.Add(new RecruiterProfile
            {
                UserId = user.Id,
                FullName = dto.FullName,
                CompanyName = "My Company" // placeholder — user can update later
            });
        }

        await _db.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateAccessToken(user, roles);

        return StatusCode(201, new AuthResponseDto(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            UserId: user.Id,
            Email: user.Email!,
            FullName: user.FullName,
            Role: user.Role
        ));
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password." });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateAccessToken(user, roles);

        return Ok(new AuthResponseDto(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            UserId: user.Id,
            Email: user.Email!,
            FullName: user.FullName,
            Role: user.Role
        ));
    }

    // GET api/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userId is null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            Roles = roles
        });
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

        if (userId is null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        // Rotating the SecurityStamp invalidates all existing JWTs for this user
        await _userManager.UpdateSecurityStampAsync(user);

        return Ok(new { message = "Logged out successfully. Your token has been invalidated." });
    }
}
