using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;
using System.Text.Json;

namespace UniversityManagement.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly UniversityDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(UniversityDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, string userId)
    {
        var notification = new Notification
        {
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ActionUrl = dto.ActionUrl,
            Data = dto.Data != null ? JsonSerializer.Serialize(dto.Data) : null
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created notification: {Title} for user {UserId}", dto.Title, userId);

        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            UserId = notification.UserId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ActionUrl = notification.ActionUrl,
            Data = notification.Data != null ? JsonSerializer.Deserialize<object>(notification.Data) : null
        };
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                UserId = n.UserId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                ActionUrl = n.ActionUrl,
                Data = n.Data != null ? JsonSerializer.Deserialize<object>(n.Data) : null
            })
            .ToListAsync();

        return notifications;
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Marked notification {NotificationId} as read", notificationId);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", notifications.Count, userId);
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted notification {NotificationId}", notificationId);
        return true;
    }

    public async Task<int> DeleteAllReadNotificationsAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.IsRead)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted {Count} read notifications for user {UserId}", notifications.Count, userId);
        return notifications.Count;
    }

    public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                UserId = n.UserId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                ActionUrl = n.ActionUrl,
                Data = n.Data != null ? JsonSerializer.Deserialize<object>(n.Data) : null
            })
            .ToListAsync();

        return notifications;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<Dictionary<string, int>> GetNotificationStatsAsync()
    {
        var stats = await _context.Notifications
            .GroupBy(n => n.Type)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        return stats;
    }

    // System notifications
    public async Task NotifyGradeAddedAsync(int studentId, string courseName, double grade)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null) return;

        var notificationDto = new CreateNotificationDto
        {
            Title = "New Grade Added",
            Message = $"You received a grade of {grade} in {courseName}",
            Type = "GradeAdded",
            ActionUrl = "/grades/my-grades",
            Data = new { CourseName = courseName, Grade = grade }
        };

        await CreateNotificationAsync(notificationDto, student.User.Id.ToString());
    }

    public async Task NotifyEnrollmentApprovedAsync(int studentId, string courseName)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        if (student == null) return;

        var notificationDto = new CreateNotificationDto
        {
            Title = "Enrollment Approved",
            Message = $"Your enrollment in {courseName} has been approved",
            Type = "EnrollmentApproved",
            ActionUrl = "/courses/my-courses",
            Data = new { CourseName = courseName }
        };

        await CreateNotificationAsync(notificationDto, student.User.Id.ToString());
    }

    public async Task NotifyCourseUpdatedAsync(string courseName, string changeDescription)
    {
        var students = await _context.StudentCourses
            .Include(sc => sc.Student)
                .ThenInclude(s => s.User)
            .Include(sc => sc.Course)
            .Select(sc => new
            {
                StudentId = sc.Student.Id,
                UserId = sc.Student.User.Id.ToString(),
                StudentName = sc.Student.FullName,
                CourseName = sc.Course.Name,
                Email = sc.Student.User.Email
            })
            .Distinct()
            .ToListAsync();

        foreach (var student in students)
        {
            var notificationDto = new CreateNotificationDto
            {
                Title = "Course Updated",
                Message = $"Course {courseName} has been updated: {changeDescription}",
                Type = "CourseUpdated",
                ActionUrl = "/courses/my-courses",
                Data = new { CourseName = courseName, ChangeDescription = changeDescription }
            };

            await CreateNotificationAsync(notificationDto, student.UserId);
        }
    }

    public async Task NotifySystemAlertAsync(string title, string message, string? actionUrl = null)
    {
        var users = await _context.Users.ToListAsync();

        foreach (var user in users)
        {
            var notificationDto = new CreateNotificationDto
            {
                Title = title,
                Message = message,
                Type = "SystemAlert",
                ActionUrl = actionUrl,
                Data = new { AlertType = title }
            };

            await CreateNotificationAsync(notificationDto, user.Id.ToString());
        }
    }

    public async Task CleanupOldNotificationsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-30); // Delete notifications older than 30 days

        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} old notifications older than {Date}", oldNotifications.Count, cutoffDate);
    }

    public async Task<NotificationStatsDto> GetSystemNotificationStatsAsync()
    {
        var notifications = await _context.Notifications.ToListAsync();

        var stats = new NotificationStatsDto
        {
            TotalNotifications = notifications.Count,
            UnreadNotifications = notifications.Count(n => !n.IsRead),
            ReadNotifications = notifications.Count(n => n.IsRead),
            NotificationsByType = notifications
                .GroupBy(n => n.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentNotifications = notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    UserId = n.UserId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt,
                    ActionUrl = n.ActionUrl,
                    Data = n.Data != null ? JsonSerializer.Deserialize<object>(n.Data) : null
                })
                .ToList(),
            LastNotification = notifications.Any() ? notifications.Max(n => n.CreatedAt) : DateTime.MinValue
        };

        return stats;
    }
}

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, string userId);
    Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(int notificationId);
    Task<int> DeleteAllReadNotificationsAsync(string userId);
    Task<List<NotificationDto>> GetUnreadNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<Dictionary<string, int>> GetNotificationStatsAsync();
    Task NotifyGradeAddedAsync(int studentId, string courseName, double grade);
    Task NotifyEnrollmentApprovedAsync(int studentId, string courseName);
    Task NotifyCourseUpdatedAsync(string courseName, string changeDescription);
    Task NotifySystemAlertAsync(string title, string message, string? actionUrl = null);
    Task CleanupOldNotificationsAsync();
    Task<NotificationStatsDto> GetSystemNotificationStatsAsync();
}
