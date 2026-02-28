using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;
using System.Security.Claims;
using System.Text.Json;

namespace UniversityManagement.Infrastructure.Services;

public class UserProfileService
{
    private readonly UniversityDbContext _context;
    private readonly ILogger<UserProfileService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserProfileService(UniversityDbContext context, ILogger<UserProfileService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserProfileDto> GetMyProfileAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        var user = await _context.Users
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        var profileDto = await BuildUserProfileDto(user);

        // Log activity
        await LogActivityAsync("ViewProfile", "User viewed their profile", "User", userId);

        return profileDto;
    }

    public async Task<UserProfileDto> UpdateMyProfileAsync(UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        var user = await _context.Users
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        var oldValues = JsonSerializer.Serialize(new { user.FullName, user.UserProfile });

        // Update user basic info
        user.FullName = dto.FullName;
        user.UpdatedAt = DateTime.UtcNow;

        // Update or create profile
        if (user.UserProfile == null)
        {
            user.UserProfile = new UserProfile
            {
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                DateOfBirth = dto.DateOfBirth,
                Bio = dto.Bio,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UserProfiles.Add(user.UserProfile);
        }
        else
        {
            user.UserProfile.PhoneNumber = dto.PhoneNumber;
            user.UserProfile.Address = dto.Address;
            user.UserProfile.DateOfBirth = dto.DateOfBirth;
            user.UserProfile.Bio = dto.Bio;
            user.UserProfile.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var newValues = JsonSerializer.Serialize(new { user.FullName, user.UserProfile });

        // Log activity
        await LogActivityAsync("UpdateProfile", "User updated their profile", "User", userId, oldValues, newValues);

        return await BuildUserProfileDto(user);
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect");

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log activity
        await LogActivityAsync("ChangePassword", "User changed their password", "User", userId);

        return true;
    }

    public async Task<FileUploadResponseDto> UploadProfilePictureAsync(IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided");

        // Validate file
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Only image files (jpg, jpeg, png, gif) are allowed");

        if (file.Length > 5 * 1024 * 1024) // 5MB limit
            throw new InvalidOperationException("File size cannot exceed 5MB");

        // Generate unique filename
        var fileName = $"profile_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine("uploads", "profiles", fileName);

        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/profiles/{fileName}";

        // Update user profile
        var user = await _context.Users
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user != null)
        {
            if (user.UserProfile == null)
            {
                user.UserProfile = new UserProfile
                {
                    ProfilePictureUrl = fileUrl,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserProfiles.Add(user.UserProfile);
            }
            else
            {
                user.UserProfile.ProfilePictureUrl = fileUrl;
                user.UserProfile.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        // Log activity
        await LogActivityAsync("UploadProfilePicture", "User uploaded profile picture", "User", userId);

        return new FileUploadResponseDto
        {
            FileName = fileName,
            FileUrl = fileUrl,
            FileSize = file.Length,
            ContentType = file.ContentType,
            FileType = "profile-picture",
            UploadedAt = DateTime.UtcNow,
            Message = "Profile picture uploaded successfully"
        };
    }

    public async Task<NotificationStatsDto> GetNotificationStatsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId.ToString())
            .ToListAsync();

        var stats = new NotificationStatsDto
        {
            TotalNotifications = notifications.Count,
            UnreadNotifications = notifications.Count(n => !n.IsRead),
            ReadNotifications = notifications.Count(n => n.IsRead),
            NotificationsByType = notifications
                .GroupBy(n => n.Type)
                .ToDictionary(g => g.Key, g.Count()),
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

    public async Task<List<NotificationDto>> GetMyNotificationsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId.ToString())
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

    public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId.ToString());

        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Log activity
        await LogActivityAsync("MarkNotificationRead", "User marked notification as read", "Notification", notificationId);

        return true;
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId.ToString() && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Log activity
        await LogActivityAsync("MarkAllNotificationsRead", $"User marked {notifications.Count} notifications as read", "Notification", userId);

        return true;
    }

    public async Task<UserStatisticsDto> GetUserStatisticsAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User not authenticated");

        var activities = await _context.ActivityLogs
            .Where(a => a.UserId == userId.ToString())
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var stats = new UserStatisticsDto
        {
            TotalLogins = activities.Count(a => a.Action == "Login"),
            LastLogin = activities.Where(a => a.Action == "Login").Select(a => a.CreatedAt).FirstOrDefault(),
            LastLoginIp = activities.Where(a => a.Action == "Login").Select(a => a.IpAddress).FirstOrDefault(),
            TotalActions = activities.Count,
            ActionsByType = activities
                .GroupBy(a => a.Action)
                .ToDictionary(g => g.Key, g.Count()),
            RecentActivities = activities.Take(10)
                .Select(a => new ActivityLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Action = a.Action,
                    Description = a.Description,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    CreatedAt = a.CreatedAt,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    OldValues = a.OldValues,
                    NewValues = a.NewValues
                })
                .ToList(),
            AverageSessionDuration = CalculateAverageSessionDuration(activities)
        };

        return stats;
    }

    public async Task<List<UserProfileDto>> SearchUsersAsync(SearchUsersDto searchDto)
    {
        var query = _context.Users
            .Include(u => u.UserProfile)
            .Include(u => u.Student)
            .Include(u => u.Teacher)
            .AsQueryable();

        // Apply role filter
        if (searchDto.Role != "All")
        {
            query = query.Where(u => u.Role == searchDto.Role);
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(searchDto.Query))
        {
            query = query.Where(u => 
                u.FullName.Contains(searchDto.Query) ||
                u.Email.Contains(searchDto.Query) ||
                (u.UserProfile != null && u.UserProfile.PhoneNumber.Contains(searchDto.Query)));
        }

        // Apply sorting
        query = searchDto.SortBy.ToLower() switch
        {
            "name" or "fullname" => searchDto.SortDescending 
                ? query.OrderByDescending(u => u.FullName)
                : query.OrderBy(u => u.FullName),
            "email" => searchDto.SortDescending 
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "createdat" => searchDto.SortDescending 
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderBy(u => u.FullName)
        };

        // Apply pagination
        var pagedQuery = query
            .Skip((searchDto.Page - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize);

        var users = await pagedQuery
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                FullName = u.FullName,
                ProfilePictureUrl = u.UserProfile?.ProfilePictureUrl,
                DepartmentName = u.Student != null ? u.Student.Department.Name : 
                              u.Teacher != null ? u.Teacher.Department.Name : null,
                PhoneNumber = u.UserProfile?.PhoneNumber,
                Address = u.UserProfile?.Address,
                DateOfBirth = u.UserProfile?.DateOfBirth,
                Bio = u.UserProfile?.Bio,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.UserProfile?.LastLoginAt ?? DateTime.MinValue,
                IsActive = u.IsActive,
                StudentId = u.Student?.Id.ToString(),
                TeacherId = u.Teacher?.Id.ToString(),
                StudentCount = u.Student != null ? 1 : 0,
                CourseCount = u.Teacher != null ? _context.Courses.Count(c => c.TeacherId == u.Teacher.Id) : 0,
                AverageGrade = u.Student != null ? _context.Grades
                    .Where(g => g.StudentId == u.Student.Id)
                    .Select(g => g.Value)
                    .DefaultIfEmpty(0)
                    .Average() : 0
            })
            .ToListAsync();

        return users;
    }

    private async Task<UserProfileDto> BuildUserProfileDto(User user)
    {
        return await Task.FromResult(new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            FullName = user.FullName,
            ProfilePictureUrl = user.UserProfile?.ProfilePictureUrl,
            DepartmentName = user.Student != null ? user.Student.Department.Name : 
                              user.Teacher != null ? user.Teacher.Department.Name : null,
            PhoneNumber = user.UserProfile?.PhoneNumber,
            Address = user.UserProfile?.Address,
            DateOfBirth = user.UserProfile?.DateOfBirth,
            Bio = user.UserProfile?.Bio,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.UserProfile?.LastLoginAt ?? DateTime.MinValue,
            IsActive = user.IsActive,
            StudentId = user.Student?.Id.ToString(),
            TeacherId = user.Teacher?.Id.ToString(),
            StudentCount = user.Student != null ? 1 : 0,
            CourseCount = user.Teacher != null ? await _context.Courses.CountAsync(c => c.TeacherId == user.Teacher.Id) : 0,
            AverageGrade = user.Student != null ? await _context.Grades
                .Where(g => g.StudentId == user.Student.Id)
                .Select(g => g.Value)
                .DefaultIfEmpty(0)
                .AverageAsync() : 0
        });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private async Task LogActivityAsync(string action, string description, string entityType, int? entityId, string oldValues = null, string newValues = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return;

            var activity = new ActivityLog
            {
                UserId = userId.ToString(),
                Action = action,
                Description = description,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues
            };

            _context.ActivityLogs.Add(activity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging activity: {Action}", action);
        }
    }

    private TimeSpan CalculateAverageSessionDuration(List<ActivityLog> activities)
    {
        var loginActivities = activities.Where(a => a.Action == "Login").ToList();
        var logoutActivities = activities.Where(a => a.Action == "Logout").ToList();

        var durations = new List<TimeSpan>();

        foreach (var login in loginActivities)
        {
            var logout = logoutActivities.FirstOrDefault(l => l.CreatedAt > login.CreatedAt);
            if (logout != null)
            {
                durations.Add(logout.CreatedAt - login.CreatedAt);
            }
        }

        return durations.Any() ? TimeSpan.FromTicks((long)durations.Average(d => d.Ticks)) : TimeSpan.Zero;
    }
}
