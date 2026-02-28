using System;

namespace UniversityManagement.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; }
    public string JwtId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? UserAgent { get; set; }

    // Navigation properties
    public User User { get; set; }

    // Helper properties
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;
}
