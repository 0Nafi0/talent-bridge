using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TalentBridge.Api.Data;
using TalentBridge.Api.DTOs.Notifications;

namespace TalentBridge.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize] // Any logged-in user can have notifications
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public NotificationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.FindFirstValue("sub") 
            ?? string.Empty;
    }

    // ── GET /api/notifications ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var dtos = notifications.Select(n => new NotificationDto(
            Id: n.Id,
            Title: n.Title,
            Message: n.Message,
            IsRead: n.IsRead,
            Type: n.Type.ToString(),
            CreatedAt: n.CreatedAt,
            RelatedEntityId: n.RelatedEntityId
        )).ToList();

        return Ok(dtos);
    }

    // ── PUT /api/notifications/{id}/read ─────────────────────────────────────
    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification is null) return NotFound(new { message = "Notification not found." });

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = "Notification marked as read." });
    }

    // ── PUT /api/notifications/read-all ──────────────────────────────────────
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        
        var unreadNotifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadNotifications.Any())
        {
            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { message = $"{unreadNotifications.Count} notification(s) marked as read." });
    }
}
