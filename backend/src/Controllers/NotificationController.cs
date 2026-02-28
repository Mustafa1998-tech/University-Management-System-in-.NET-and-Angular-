using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    // Get my notifications
    [HttpGet("my-notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        try
        {
            var userId = User.FindFirst("UserId")?.Value ?? "";
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(ApiResponseDto<List<NotificationDto>.SuccessResult(notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return StatusCode(500, ApiResponseDto<List<NotificationDto>.ErrorResult("Failed to get notifications"));
        }
    }

    // Get unread notifications
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        try
        {
            var userId = User.FindFirst("UserId")?.Value ?? "";
            var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
            return Ok(ApiResponseDto<List<NotificationDto>.SuccessResult(notifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notifications");
            return StatusCode(500, ApiResponseDto<List<NotificationDto>.ErrorResult("Failed to get unread notifications"));
        }
    }

    // Get unread count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = User.FindFirst("UserId")?.Value ?? "";
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(ApiResponseDto<int>.SuccessResult(count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread notification count");
            return StatusCode(500, ApiResponseDto<int>.ErrorResult("Failed to get unread count"));
        }
    }

    // Mark notification as read
    [HttpPost("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            
            if (result)
            {
                return Ok(ApiResponseDto<string>.SuccessResult("Notification marked as read"));
            }
            else
            {
                return NotFound(ApiResponseDto<string>.ErrorResult("Notification not found"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to mark notification as read"));
        }
    }

    // Mark all as read
    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = User.FindFirst("UserId")?.Value ?? "";
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            
            if (result)
            {
                return Ok(ApiResponseDto<string>.SuccessResult("All notifications marked as read"));
            }
            else
            {
                return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to mark all notifications as read"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to mark all notifications as read"));
        }
    }

    // Delete notification
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        try
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            
            if (result)
            {
                return Ok(ApiResponseDto<string>.SuccessResult("Notification deleted successfully"));
            }
            else
            {
                return NotFound(ApiResponseDto<string>.ErrorResult("Notification not found"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to delete notification"));
        }
    }

    // Delete all read notifications
    [HttpDelete("read")]
    public async Task<IActionResult> DeleteAllReadNotifications()
    {
        try
        {
            var userId = User.FindFirst("UserId")?.Value ?? "";
            var deletedCount = await _notificationService.DeleteAllReadNotificationsAsync(userId);
            
            return Ok(ApiResponseDto<int>.SuccessResult(deletedCount, $"Deleted {deletedCount} read notifications"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting read notifications");
            return StatusCode(500, ApiResponseDto<int>.ErrorResult("Failed to delete read notifications"));
        }
    }

    // Create notification (Admin only)
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto, [FromQuery] string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponseDto<NotificationDto>.ErrorResult("UserId is required"));
            }

            var notification = await _notificationService.CreateNotificationAsync(dto, userId);
            return Ok(ApiResponseDto<NotificationDto>.SuccessResult(notification, "Notification created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification");
            return StatusCode(500, ApiResponseDto<NotificationDto>.ErrorResult("Failed to create notification"));
        }
    }

    // Get notification statistics (Admin only)
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetNotificationStats()
    {
        try
        {
            var stats = await _notificationService.GetSystemNotificationStatsAsync();
            return Ok(ApiResponseDto<NotificationStatsDto>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification stats");
            return StatusCode(500, ApiResponseDto<NotificationStatsDto>.ErrorResult("Failed to get notification stats"));
        }
    }

    // Get notification types (Admin only)
    [HttpGet("types")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetNotificationTypes()
    {
        try
        {
            var types = await _notificationService.GetNotificationStatsAsync();
            return Ok(ApiResponseDto<Dictionary<string, int>>.SuccessResult(types.NotificationsByType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification types");
            return StatusCode(500, ApiResponseDto<Dictionary<string, int>>.ErrorResult("Failed to get notification types"));
        }
    }

    // Cleanup old notifications (Admin only)
    [HttpPost("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupOldNotifications()
    {
        try
        {
            await _notificationService.CleanupOldNotificationsAsync();
            return Ok(ApiResponseDto<string>.SuccessResult("Old notifications cleaned up successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old notifications");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to clean up old notifications"));
        }
    }

    // Send grade notification (Admin only)
    [HttpPost("send-grade-notification")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendGradeNotification([FromBody] GradeNotificationDto dto)
    {
        try
        {
            await _notificationService.NotifyGradeAddedAsync(dto.StudentId, dto.CourseName, dto.Grade);
            return Ok(ApiResponseDto<string>.SuccessResult("Grade notification sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending grade notification");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to send grade notification"));
        }
    }

    // Send enrollment notification (Admin only)
    [HttpPost("send-enrollment-notification")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendEnrollmentNotification([FromBody] EnrollmentNotificationDto dto)
    {
        try
        {
            await _notificationService.NotifyEnrollmentApprovedAsync(dto.StudentId, dto.CourseName);
            return Ok(ApiResponseDto<string>.SuccessResult("Enrollment notification sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending enrollment notification");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to send enrollment notification"));
        }
    }

    // Send system alert (Admin only)
    [HttpPost("send-system-alert")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendSystemAlert([FromBody] SystemAlertDto dto)
    {
        try
        {
            await _notificationService.NotifySystemAlertAsync(dto.Title, dto.Message, dto.ActionUrl);
            return Ok(ApiResponseDto<string>.SuccessResult("System alert sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system alert");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to send system alert"));
        }
    }

    // Notification documentation
    [HttpGet("docs")]
    [AllowAnonymous]
    public IActionResult GetNotificationDocumentation()
    {
        var docs = new
        {
            Title = "Notification API Documentation",
            Version = "1.0.0",
            Description = "Real-time notification system for University Management System",
            Endpoints = new[]
            {
                new { Method = "GET", Path = "/api/notification/my-notifications", Description = "Get current user's notifications" },
                new { Method = "GET", Path = "/api/notification/unread", Description = "Get unread notifications" },
                new { Method = "GET", Path = "/api/notification/unread-count", Description = "Get unread notification count" },
                new { Method = "POST", Path = "/api/notification/{id}/mark-read", Description = "Mark notification as read" },
                new { Method = "POST", Path = "/api/notification/mark-all-read", Description = "Mark all notifications as read" },
                new { Method = "DELETE", Path = "/api/notification/{id}", Description = "Delete notification" },
                new { Method = "DELETE", Path = "/api/notification/read", Description = "Delete all read notifications" },
                new { Method = "POST", Path = "/api/notification/create", Description = "Create notification (Admin only)" },
                new { Method = "GET", Path = "/api/notification/stats", Description = "Get notification statistics (Admin only)" },
                new { Method = "GET", Path = "/api/notification/types", Description = "Get notification types (Admin only)" },
                new { Method = "POST", Path = "/api/notification/cleanup", Description = "Cleanup old notifications (Admin only)" }
            },
            NotificationTypes = new[]
            {
                new { Type = "Info", Description = "Informational notifications" },
                new { Type = "Success", Description = "Success notifications" },
                new { Type = "Warning", Description = "Warning notifications" },
                new { Type = "Error", Description = "Error notifications" },
                new { Type = "GradeAdded", Description = "When a new grade is added" },
                new { Type = "EnrollmentApproved", Description = "When enrollment is approved" },
                new { Type = "CourseUpdated", Description = "When a course is updated" },
                new { Type = "SystemAlert", Description = "System-wide alerts" }
            },
            Features = new
            {
                RealTimeNotifications = "Real-time notification system",
                UserSpecific = "User-specific notifications",
                BulkOperations = "Bulk mark as read/delete operations",
                Statistics = "Comprehensive notification statistics",
                SystemAlerts = "System-wide alert system",
                AutomaticCleanup = "Automatic cleanup of old notifications",
                TypeBasedFiltering = "Filter notifications by type",
                ActionUrls = "Clickable notification actions"
            },
            Examples = new[]
            {
                new
                {
                    Description = "Get user notifications",
                    Url = "/api/notification/my-notifications",
                    Method = "GET"
                },
                new
                {
                    Description = "Mark notification as read",
                    Url = "/api/notification/123/mark-read",
                    Method = "POST"
                },
                new
                {
                    Description = "Send grade notification",
                    Url = "/api/notification/send-grade-notification",
                    Method = "POST",
                    Body = new { StudentId = 1, CourseName = "Math 101", Grade = 85.5 }
                },
                new
                {
                    Description = "Send system alert",
                    Url = "/api/notification/send-system-alert",
                    Method = "POST",
                    Body = new { Title = "System Maintenance", Message = "System will be down for maintenance" }
                }
            },
            BestPractices = new[]
            {
                "Use meaningful titles and messages",
                "Include action URLs when applicable",
                "Clean up old notifications regularly",
                "Use appropriate notification types",
                "Limit notification frequency",
                "Provide clear action descriptions"
            }
        };

        return Ok(docs);
    }

    // Health check
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "NotificationController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "Real-time Notifications",
                "User Management",
                "System Alerts",
                "Statistics Tracking",
                "Automatic Cleanup",
                "Type-based Filtering",
                "Action URL Support"
            }
        });
    }
}

// Supporting DTOs for notification endpoints
public class GradeNotificationDto
{
    public int StudentId { get; set; }
    public string CourseName { get; set; }
    public double Grade { get; set; }
}

public class EnrollmentNotificationDto
{
    public int StudentId { get; set; }
    public string CourseName { get; set; }
}

public class SystemAlertDto
{
    public string Title { get; set; }
    public string Message { get; set; }
    public string? ActionUrl { get; set; }
}
