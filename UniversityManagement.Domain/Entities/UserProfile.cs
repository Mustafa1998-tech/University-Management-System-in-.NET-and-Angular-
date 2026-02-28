using System;

namespace UniversityManagement.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Bio { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string TimeZone { get; set; }
    public string Language { get; set; }
    public string Theme { get; set; }
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool ProfileVisibility { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public string LastLoginIp { get; set; }
    public string LastLoginUserAgent { get; set; }

    // Navigation properties
    public int UserId { get; set; }
    public User User { get; set; }
}
