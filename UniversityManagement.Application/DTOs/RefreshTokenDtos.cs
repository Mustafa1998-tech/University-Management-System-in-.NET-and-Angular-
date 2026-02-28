using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Application.DTOs;

// Refresh token request
public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; }
}

// Refresh token response
public class RefreshTokenResponseDto
{
    public string JwtToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Role { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public TimeSpan RefreshTokenExpiresIn { get; set; }
}

// Token pair response
public class TokenPairDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public string TokenType { get; set; }
    public TimeSpan ExpiresIn { get; set; }
}

// Revoke token request
public class RevokeTokenRequestDto
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; }
    
    [Required(ErrorMessage = "Reason is required")]
    [MinLength(3, ErrorMessage = "Reason must be at least 3 characters")]
    public string Reason { get; set; }
}

// Token information
public class TokenInfoDto
{
    public string TokenId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? UserAgent { get; set; }
    public TimeSpan TimeUntilExpiry { get; set; }
}

// Token statistics
public class TokenStatsDto
{
    public int TotalTokens { get; set; }
    public int ActiveTokens { get; set; }
    public int ExpiredTokens { get; set; }
    public int RevokedTokens { get; set; }
    public int UsedTokens { get; set; }
    public int TokensCreatedToday { get; set; }
    public int TokensCreatedThisWeek { get; set; }
    public int TokensCreatedThisMonth { get; set; }
    public double AverageTokenLifetime { get; set; }
    public List<TokenActivityDto> RecentActivity { get; set; }
}

public class TokenActivityDto
{
    public DateTime Date { get; set; }
    public int TokensCreated { get; set; }
    public int TokensRevoked { get; set; }
    public int TokensUsed { get; set; }
    public int ActiveTokensCount { get; set; }
}

// Token validation result
public class TokenValidationResultDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
    public string ErrorCode { get; set; }
    public TokenInfoDto TokenInfo { get; set; }
    public DateTime ValidatedAt { get; set; }
}

// Token management settings
public class TokenSettingsDto
{
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public int JwtExpirationMinutes { get; set; } = 60;
    public int MaxActiveTokensPerUser { get; set; } = 5;
    public bool RevokeOldTokensOnNewLogin { get; set; } = true;
    public bool RequireUserAgentValidation { get; set; } = false;
    public bool TrackTokenUsage { get; set; } = true;
    public string[] AllowedOrigins { get; set; } = new string[] { "*" };
}

// Token cleanup result
public class TokenCleanupResultDto
{
    public int ExpiredTokensRemoved { get; set; }
    public int RevokedTokensRemoved { get; set; }
    public int UsedTokensRemoved { get; set; }
    public TimeSpan CleanupTime { get; set; }
    public DateTime CleanupAt { get; set; }
    public string Message { get; set; }
}
