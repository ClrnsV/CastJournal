using CastJournal.Application.DTOs.Notifications;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace CastJournal.API.Controllers;

[Route("api/notifications")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] NotificationFilterDto filter)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var result = await _notificationService.GetNotificationsAsync(userId, filter);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { unreadCount = count });
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        var success = await _notificationService.MarkAsReadAsync(id, userId);
        if (!success) return NotFound("Notification not found or unauthorized");

        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized("User ID not found in token. Please login again.");

        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }
}