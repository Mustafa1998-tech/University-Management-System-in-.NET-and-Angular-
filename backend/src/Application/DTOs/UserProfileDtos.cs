using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Application.DTOs;

// User Profile DTO
public class UserProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string FullName { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string DepartmentName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public string StudentId { get; set; }
    public string TeacherId { get; set; }
    public int? StudentCount { get; set; set; }
    public int? CourseCount { get; set; set; }
    public double? AverageGrade { get; set; }
}

// Update Profile DTO
public class UpdateProfileDto
{
    [Required(ErrorMessage = "Full name is required")]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string PhoneNumber { get; set; }

    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string Address { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    public string Bio { get; set; }
}

// Change Password DTO
public class ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required")]
    [MinLength(1, ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}

// File Upload DTO
public class FileUploadDto
{
    public IFormFile File { get; set; }
    public string FileType { get; set; } // profile-picture, document, etc.
    public string Description { get; set; }
}

// File Upload Response
public class FileUploadResponseDto
{
    public string FileName { get; set; }
    public string FileUrl { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public string FileType { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Message { get; set; }
}

// Notification DTOs
public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
    public string UserId { get; set; }
    public bool IsRead { get; set; set; }
    public DateTime CreatedAt { get; set; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public object? Data { get; set; }
}

public class CreateNotificationDto
{
    [Required(ErrorMessage = "Title is required")]
    [MinLength(3, ErrorMessage = "Title must be at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Message is required")]
    [MinLength(5, ErrorMessage = "Message must be at least 5 characters")]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; }

    [Required(ErrorMessage = "Type is required")]
    [RegularExpression(@"^(Info|Success|Warning|Error|GradeAdded|EnrollmentApproved|CourseUpdated|SystemAlert)$", 
        ErrorMessage = "Invalid notification type")]
    public string Type { get; set; }

    public string? ActionUrl { get; set; }
    public object? Data { get; set; }
}

public class NotificationStatsDto
{
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public int ReadNotifications { get; set; }
    public Dictionary<string, int> NotificationsByType { get; set; }
    public List<NotificationDto> RecentNotifications { get; set; }
    public DateTime LastNotification { get; set; }
}

// Activity Log DTO
public class ActivityLogDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
}

// User Statistics DTO
public class UserStatisticsDto
{
    public int TotalLogins { get; set; }
    public DateTime LastLogin { get; set; }
    public string LastLoginIp { get; set; }
    public int TotalActions { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; }
    public List<ActivityLogDto> RecentActivities { get; set; }
    public TimeSpan AverageSessionDuration { get; set; }
}

// Settings DTO
public class UserSettingsDto
{
    public string Theme { get; set; } = "light"
    public string Language { get; set; } = "en"
    public bool EmailNotifications { get; set; } = true
    public bool PushNotifications { get; set; } = true
    public bool ProfileVisibility { get; set; } = true
    public string TimeZone { get; set; } = "UTC"
    public int ItemsPerPage { get; set; } = 10
    public string DateFormat { get; set; } = "MM/dd/yyyy"
}

// Search Users DTO
public class SearchUsersDto
{
    [MinLength(2, ErrorMessage = "Search term must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string Query { get; set; }

    [RegularExpression(@"^(Student|Teacher|Admin|All)$", ErrorMessage = "Role must be Student, Teacher, Admin, or All")]
    public string Role { get; set; } = "All";

    [Range(1, 100, ErrorMessage = "Page must be between 1 and 100")]
    public int Page { get; set; } = 1;

    [Range(5, 50, ErrorMessage = "Page size must be between 5 and 50")]
    public int PageSize { get; set; } = 10;

    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = false;
}
