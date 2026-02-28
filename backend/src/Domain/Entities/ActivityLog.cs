using System;

namespace UniversityManagement.Entities;

public class ActivityLog
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Action { get; set; } // Login, Logout, Update, Delete, Create, etc.
    public string Description { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? EntityType { get; set; } // User, Student, Teacher, Course, Grade, etc.
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    // Navigation properties
    public User User { get; set; }
}
