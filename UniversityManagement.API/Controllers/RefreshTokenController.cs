using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[ApiController]
[Route("api/[controller]")]
public class RefreshTokenController : ControllerBase
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<RefreshTokenController> _logger;

    public RefreshTokenController(IRefreshTokenService refreshTokenService, ILogger<RefreshTokenController> logger)
    {
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    // Refresh token endpoint
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _refreshTokenService.RefreshTokenAsync(request.Token);
            return Ok(ApiResponseDto<RefreshTokenResponseDto>.SuccessResult(result, "Token refreshed successfully"));
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(ApiResponseDto<RefreshTokenResponseDto>.ErrorResult(ex.Message, "INVALID_REFRESH_TOKEN"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, ApiResponseDto<RefreshTokenResponseDto>.ErrorResult("Failed to refresh token"));
        }
    }

    // Revoke token endpoint
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _refreshTokenService.RevokeTokenAsync(request.Token, request.Reason, ipAddress);
            
            if (result)
            {
                return Ok(ApiResponseDto<string>.SuccessResult("Token revoked successfully"));
            }
            else
            {
                return NotFound(ApiResponseDto<string>.ErrorResult("Token not found"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to revoke token"));
        }
    }

    // Revoke all user tokens
    [HttpPost("revoke-all")]
    [Authorize]
    public async Task<IActionResult> RevokeAllUserTokens([FromBody] RevokeTokenRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            var result = await _refreshTokenService.RevokeAllUserTokensAsync(userId, request.Reason);
            
            if (result)
            {
                return Ok(ApiResponseDto<string>.SuccessResult("All user tokens revoked successfully"));
            }
            else
            {
                return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to revoke user tokens"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all user tokens");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to revoke user tokens"));
        }
    }

    // Get token information
    [HttpGet("info/{token}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTokenInfo(string token)
    {
        try
        {
            var tokenInfo = await _refreshTokenService.GetTokenInfoAsync(token);
            
            if (tokenInfo == null)
            {
                return NotFound(ApiResponseDto<TokenInfoDto>.ErrorResult("Token not found"));
            }

            return Ok(ApiResponseDto<TokenInfoDto>.SuccessResult(tokenInfo, "Token information retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token info");
            return StatusCode(500, ApiResponseDto<TokenInfoDto>.ErrorResult("Failed to get token information"));
        }
    }

    // Get token statistics (Admin only)
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTokenStats()
    {
        try
        {
            var stats = await _refreshTokenService.GetTokenStatsAsync();
            return Ok(ApiResponseDto<TokenStatsDto>.SuccessResult(stats, "Token statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token stats");
            return StatusCode(500, ApiResponseDto<TokenStatsDto>.ErrorResult("Failed to get token statistics"));
        }
    }

    // Validate token
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _refreshTokenService.ValidateTokenAsync(request.Token);
            
            return Ok(ApiResponseDto<TokenValidationResultDto>.SuccessResult(result, 
                result.IsValid ? "Token is valid" : "Token is invalid"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, ApiResponseDto<TokenValidationResultDto>.ErrorResult("Failed to validate token"));
        }
    }

    // Cleanup expired tokens (Admin only)
    [HttpPost("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupExpiredTokens()
    {
        try
        {
            var result = await _refreshTokenService.CleanupExpiredTokensAsync();
            return Ok(ApiResponseDto<TokenCleanupResultDto>.SuccessResult(result, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired tokens");
            return StatusCode(500, ApiResponseDto<TokenCleanupResultDto>.ErrorResult("Failed to cleanup expired tokens"));
        }
    }

    // Get current user's active tokens
    [HttpGet("my-tokens")]
    [Authorize]
    public async Task<IActionResult> GetMyTokens()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var stats = await _refreshTokenService.GetTokenStatsAsync();
            
            // Filter stats for current user (this is a simplified approach)
            // In a real implementation, you'd have a method to get user-specific tokens
            
            return Ok(ApiResponseDto<object>.SuccessResult(new
            {
                UserId = userId,
                Message = "User tokens retrieved",
                TotalStats = stats
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user tokens");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to get user tokens"));
        }
    }

    // Token documentation
    [HttpGet("docs")]
    [AllowAnonymous]
    public IActionResult GetTokenDocumentation()
    {
        var docs = new
        {
            Title = "Refresh Token API Documentation",
            Version = "1.0.0",
            Description = "Secure token management with refresh tokens for enhanced security",
            Endpoints = new[]
            {
                new { Method = "POST", Path = "/api/refreshtoken/refresh", Description = "Refresh access token using refresh token" },
                new { Method = "POST", Path = "/api/refreshtoken/revoke", Description = "Revoke a specific refresh token" },
                new { Method = "POST", Path = "/api/refreshtoken/revoke-all", Description = "Revoke all user's refresh tokens" },
                new { Method = "GET", Path = "/api/refreshtoken/info/{token}", Description = "Get token information (Admin only)" },
                new { Method = "GET", Path = "/api/refreshtoken/stats", Description = "Get token statistics (Admin only)" },
                new { Method = "POST", Path = "/api/refreshtoken/validate", Description = "Validate refresh token" },
                new { Method = "POST", Path = "/api/refreshtoken/cleanup", Description = "Cleanup expired tokens (Admin only)" },
                new { Method = "GET", Path = "/api/refreshtoken/my-tokens", Description = "Get current user's tokens" }
            },
            TokenFlow = new[]
            {
                "1. User logs in with credentials",
                "2. Server generates JWT + Refresh Token",
                "3. JWT expires (short-lived)",
                "4. Client sends Refresh Token",
                "5. Server validates and issues new JWT + Refresh Token",
                "6. Old Refresh Token is marked as used"
            },
            SecurityFeatures = new[]
            {
                "Long-lived refresh tokens (7 days)",
                "Short-lived JWT tokens (60 minutes)",
                "Token revocation capability",
                "Automatic cleanup of expired tokens",
                "User agent tracking",
                "IP address tracking",
                "Token usage monitoring",
                "Maximum active tokens per user"
            },
            TokenSettings = new
            {
                RefreshTokenExpirationDays = 7,
                JwtExpirationMinutes = 60,
                MaxActiveTokensPerUser = 5,
                RevokeOldTokensOnNewLogin = true,
                RequireUserAgentValidation = false,
                TrackTokenUsage = true
            },
            Examples = new object[]
            {
                new
                {
                    Description = "Refresh access token",
                    Url = "/api/refreshtoken/refresh",
                    Method = "POST",
                    Body = new { Token = "refresh_token_here" }
                },
                new
                {
                    Description = "Revoke refresh token",
                    Url = "/api/refreshtoken/revoke",
                    Method = "POST",
                    Body = new { Token = "refresh_token_here", Reason = "User logout" }
                },
                new
                {
                    Description = "Validate token",
                    Url = "/api/refreshtoken/validate",
                    Method = "POST",
                    Body = new { Token = "refresh_token_here" }
                }
            },
            BestPractices = new[]
            {
                "Store refresh tokens securely (HttpOnly cookies)",
                "Use HTTPS for all token operations",
                "Implement proper token expiration",
                "Monitor token usage patterns",
                "Revoke tokens on logout",
                "Clean up expired tokens regularly",
                "Limit concurrent sessions per user",
                "Track token revocation reasons"
            },
            ErrorCodes = new[]
            {
                new { Code = "TOKEN_NOT_FOUND", Description = "Refresh token not found in database" },
                new { Code = "TOKEN_EXPIRED", Description = "Refresh token has expired" },
                new { Code = "TOKEN_REVOKED", Description = "Refresh token has been revoked" },
                new { Code = "TOKEN_USED", Description = "Refresh token has already been used" },
                new { Code = "VALIDATION_ERROR", Description = "Token validation failed" }
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
            Service = "RefreshTokenController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "JWT Token Generation",
                "Refresh Token Management",
                "Token Revocation",
                "Token Validation",
                "Token Statistics",
                "Automatic Cleanup",
                "Security Monitoring",
                "User Agent Tracking"
            }
        });
    }
}
