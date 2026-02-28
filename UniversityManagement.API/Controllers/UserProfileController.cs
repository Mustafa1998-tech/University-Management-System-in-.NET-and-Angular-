using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly UserProfileService _userProfileService;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(UserProfileService userProfileService, ILogger<UserProfileController> logger)
    {
        _userProfileService = userProfileService;
        _logger = logger;
    }

    // Get my profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var profile = await _userProfileService.GetMyProfileAsync();
            return Ok(ApiResponseDto<UserProfileDto>.SuccessResult(profile, "Profile retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<UserProfileDto>.ErrorResult(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponseDto<UserProfileDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, ApiResponseDto<UserProfileDto>.ErrorResult("Failed to retrieve profile"));
        }
    }

    // Update my profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var profile = await _userProfileService.UpdateMyProfileAsync(dto);
            return Ok(ApiResponseDto<UserProfileDto>.SuccessResult(profile, "Profile updated successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<UserProfileDto>.ErrorResult(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponseDto<UserProfileDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, ApiResponseDto<UserProfileDto>.ErrorResult("Failed to update profile"));
        }
    }

    // Change password
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var result = await _userProfileService.ChangePasswordAsync(dto);
            
            if (result)
            {
                return Ok(ApiResponseDto<string>.SuccessResult("Password changed successfully"));
            }
            else
            {
                return BadRequest(ApiResponseDto<string>.ErrorResult("Failed to change password"));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<string>.ErrorResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<string>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to change password"));
        }
    }

    // Upload profile picture
    [HttpPost("upload-profile-picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        try
        {
            var result = await _userProfileService.UploadProfilePictureAsync(file);
            return Ok(ApiResponseDto<FileUploadResponseDto>.SuccessResult(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<FileUploadResponseDto>.ErrorResult(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponseDto<FileUploadResponseDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile picture");
            return StatusCode(500, ApiResponseDto<FileUploadResponseDto>.ErrorResult("Failed to upload profile picture"));
        }
    }

    // Get notification statistics
    [HttpGet("notifications/stats")]
    public async Task<IActionResult> GetNotificationStats()
    {
        try
        {
            var stats = await _userProfileService.GetNotificationStatsAsync();
            return Ok(ApiResponseDto<NotificationStatsDto>.SuccessResult(stats));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<NotificationStatsDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification stats");
            return StatusCode(500, ApiResponseDto<NotificationStatsDto>.ErrorResult("Failed to get notification stats"));
        }
    }

    // Get my notifications
    [HttpGet("notifications")]
    public async Task<IActionResult> GetMyNotifications()
    {
        try
        {
            var notifications = await _userProfileService.GetMyNotificationsAsync();
            return Ok(ApiResponseDto<List<NotificationDto>>.SuccessResult(notifications));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<List<NotificationDto>>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return StatusCode(500, ApiResponseDto<List<NotificationDto>>.ErrorResult("Failed to get notifications"));
        }
    }

    // Mark notification as read
    [HttpPost("notifications/{notificationId}/mark-read")]
    public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
    {
        try
        {
            var result = await _userProfileService.MarkNotificationAsReadAsync(notificationId);
            
            if (result)
            {
                return Ok(ApiResponseDto<string>.SuccessResult("Notification marked as read"));
            }
            else
            {
                return NotFound(ApiResponseDto<string>.ErrorResult("Notification not found"));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<string>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to mark notification as read"));
        }
    }

    // Mark all notifications as read
    [HttpPost("notifications/mark-all-read")]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        try
        {
            var result = await _userProfileService.MarkAllNotificationsAsReadAsync();
            
            if (result)
            {
                return Ok(ApiResponseDto<string>.SuccessResult("All notifications marked as read"));
            }
            else
            {
                return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to mark notifications as read"));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<string>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to mark all notifications as read"));
        }
    }

    // Get user statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetUserStatistics()
    {
        try
        {
            var stats = await _userProfileService.GetUserStatisticsAsync();
            return Ok(ApiResponseDto<UserStatisticsDto>.SuccessResult(stats));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponseDto<UserStatisticsDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            return StatusCode(500, ApiResponseDto<UserStatisticsDto>.ErrorResult("Failed to get user statistics"));
        }
    }

    // Search users (Admin only)
    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchUsers([FromQuery] SearchUsersDto searchDto)
    {
        try
        {
            var users = await _userProfileService.SearchUsersAsync(searchDto);
            return Ok(ApiResponseDto<List<UserProfileDto>>.SuccessResult(users));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return StatusCode(500, ApiResponseDto<List<UserProfileDto>>.ErrorResult("Failed to search users"));
        }
    }

    // Profile documentation
    [HttpGet("docs")]
    [AllowAnonymous]
    public IActionResult GetProfileDocumentation()
    {
        var docs = new
        {
            Title = "User Profile API Documentation",
            Version = "1.0.0",
            Description = "User profile management with notifications and file uploads",
            Endpoints = new[]
            {
                new { Method = "GET", Path = "/api/userprofile/profile", Description = "Get current user profile" },
                new { Method = "PUT", Path = "/api/userprofile/profile", Description = "Update user profile" },
                new { Method = "POST", Path = "/api/userprofile/change-password", Description = "Change user password" },
                new { Method = "POST", Path = "/api/userprofile/upload-profile-picture", Description = "Upload profile picture" },
                new { Method = "GET", Path = "/api/userprofile/notifications/stats", Description = "Get notification statistics" },
                new { Method = "GET", Path = "/api/userprofile/notifications", Description = "Get user notifications" },
                new { Method = "POST", Path = "/api/userprofile/notifications/{id}/mark-read", Description = "Mark notification as read" },
                new { Method = "POST", Path = "/api/userprofile/notifications/mark-all-read", Description = "Mark all notifications as read" },
                new { Method = "GET", Path = "/api/userprofile/statistics", Description = "Get user statistics" },
                new { Method = "GET", Path = "/api/userprofile/search", Description = "Search users (Admin only)" }
            },
            Features = new
            {
                ProfileManagement = "Complete profile management",
                FileUploads = "Profile picture uploads with validation",
                Notifications = "Real-time notification system",
                ActivityLogging = "User activity tracking",
                Statistics = "Comprehensive user statistics",
                Search = "User search with filtering",
                Security = "Role-based access control"
            },
            FileUploadSettings = new
            {
                MaxFileSize = "5MB",
                AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" },
                StorageLocation = "uploads/profiles/",
                ValidationRules = new[]
                {
                    "File size must be less than 5MB",
                    "Only image files are allowed",
                    "File must be one of: jpg, jpeg, png, gif"
                }
            },
            NotificationTypes = new[]
            {
                "Info", "Success", "Warning", "Error",
                "GradeAdded", "EnrollmentApproved", "CourseUpdated", "SystemAlert"
            },
            ActivityTypes = new[]
            {
                "Login", "Logout", "Update", "Delete", "Create", "ViewProfile",
                "ChangePassword", "UploadProfilePicture", "MarkNotificationRead"
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
            Service = "UserProfileController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "Profile Management",
                "File Uploads",
                "Notifications",
                "Activity Logging",
                "User Statistics",
                "Search Functionality",
                "Security Features"
            }
        });
    }
}
