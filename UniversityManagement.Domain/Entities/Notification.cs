using System;

namespace UniversityManagement.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; } // Info, Success, Warning, Error, GradeAdded, EnrollmentApproved, etc.
    public string UserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? Data { get; set; } // JSON data for additional information

    // Navigation properties
    public User User { get; set; }
}
