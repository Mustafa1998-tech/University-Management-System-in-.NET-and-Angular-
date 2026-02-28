using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly UniversityDbContext _context;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RefreshTokenService(
        UniversityDbContext context,
        ILogger<RefreshTokenService> logger,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TokenPairDto> GenerateTokenPairAsync(User user)
    {
        try
        {
            // Generate JWT token
            var jwtToken = GenerateJwtToken(user);
            
            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(GetTokenSettings().RefreshTokenExpirationDays);
            
            try
            {
                // Persist refresh token only if schema is available.
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    JwtId = jwtToken.Id,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = refreshTokenExpiresAt,
                    UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()
                };

                await _context.RefreshTokens.AddAsync(refreshTokenEntity);
                
                if (GetTokenSettings().RevokeOldTokensOnNewLogin)
                {
                    await RevokeOldTokensAsync(user.Id);
                }

                await _context.SaveChangesAsync();
                refreshTokenExpiresAt = refreshTokenEntity.ExpiresAt;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Refresh token persistence skipped for user {UserId}", user.Id);
            }

            _logger.LogInformation("Generated new token pair for user {UserId}", user.Id);

            return new TokenPairDto
            {
                AccessToken = jwtToken.Token,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = jwtToken.ExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                TokenType = "Bearer",
                ExpiresIn = jwtToken.ExpiresAt - DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token pair for user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Find the refresh token
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", refreshToken.Substring(0, Math.Min(10, refreshToken.Length)) + "...");
                throw new SecurityTokenException("Invalid refresh token");
            }

            // Check if token is active
            if (!storedToken.IsActive)
            {
                var reason = storedToken.IsExpired ? "expired" : 
                           storedToken.IsRevoked ? "revoked" : 
                           storedToken.IsUsed ? "used" : "invalid";
                
                _logger.LogWarning("Refresh token is {Reason} for user {UserId}", reason, storedToken.UserId);
                throw new SecurityTokenException($"Refresh token is {reason}");
            }

            // Get user
            var user = storedToken.User;
            if (user == null)
            {
                _logger.LogError("User not found for refresh token {TokenId}", storedToken.Id);
                throw new SecurityTokenException("User not found");
            }

            // Mark the old token as used
            storedToken.IsUsed = true;
            _context.RefreshTokens.Update(storedToken);

            // Generate new token pair
            var newTokenPair = await GenerateTokenPairAsync(user);

            // Set the replaced by token reference
            storedToken.ReplacedByToken = newTokenPair.RefreshToken;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refreshed token for user {UserId}", user.Id);

            return new RefreshTokenResponseDto
            {
                JwtToken = newTokenPair.AccessToken,
                RefreshToken = newTokenPair.RefreshToken,
                ExpiresAt = newTokenPair.AccessTokenExpiresAt,
                Role = user.Role,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                RefreshTokenExpiresIn = newTokenPair.RefreshTokenExpiresAt - DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw;
        }
    }

    public async Task<bool> RevokeTokenAsync(string token, string reason = null, string revokedByIp = null)
    {
        try
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (storedToken == null)
            {
                _logger.LogWarning("Attempted to revoke non-existent refresh token");
                return false;
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = revokedByIp ?? _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revoked for user {UserId}. Reason: {Reason}", storedToken.UserId, reason ?? "Manual revocation");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            return false;
        }
    }

    public async Task<bool> RevokeAllUserTokensAsync(int userId, string reason = null)
    {
        try
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked {Count} active tokens for user {UserId}. Reason: {Reason}", 
                userTokens.Count, userId, reason ?? "Bulk revocation");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
            return false;
        }
    }

    public async Task<TokenInfoDto> GetTokenInfoAsync(string token)
    {
        try
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);

            if (storedToken == null)
            {
                return null;
            }

            return new TokenInfoDto
            {
                TokenId = storedToken.Id.ToString(),
                UserId = storedToken.UserId,
                UserEmail = storedToken.User.Email,
                UserRole = storedToken.User.Role,
                CreatedAt = storedToken.CreatedAt,
                ExpiresAt = storedToken.ExpiresAt,
                IsActive = storedToken.IsActive,
                IsExpired = storedToken.IsExpired,
                IsRevoked = storedToken.IsRevoked,
                IsUsed = storedToken.IsUsed,
                RevokedAt = storedToken.RevokedAt,
                RevokedByIp = storedToken.RevokedByIp,
                UserAgent = storedToken.UserAgent,
                TimeUntilExpiry = storedToken.ExpiresAt - DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token info");
            throw;
        }
    }

    public async Task<TokenStatsDto> GetTokenStatsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddDays(-30);

            var allTokens = await _context.RefreshTokens.ToListAsync();

            var stats = new TokenStatsDto
            {
                TotalTokens = allTokens.Count,
                ActiveTokens = allTokens.Count(t => t.IsActive),
                ExpiredTokens = allTokens.Count(t => t.IsExpired),
                RevokedTokens = allTokens.Count(t => t.IsRevoked),
                UsedTokens = allTokens.Count(t => t.IsUsed),
                TokensCreatedToday = allTokens.Count(t => t.CreatedAt >= today),
                TokensCreatedThisWeek = allTokens.Count(t => t.CreatedAt >= weekAgo),
                TokensCreatedThisMonth = allTokens.Count(t => t.CreatedAt >= monthAgo),
                AverageTokenLifetime = allTokens.Any() ? 
                    allTokens.Where(t => t.IsRevoked || t.IsUsed || t.IsExpired)
                        .Average(t => ((t.RevokedAt ?? t.ExpiresAt) - t.CreatedAt).TotalDays) : 0,
                RecentActivity = GetRecentActivity(allTokens)
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token stats");
            throw;
        }
    }

    public async Task<TokenCleanupResultDto> CleanupExpiredTokensAsync()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.IsExpired || rt.IsRevoked || rt.IsUsed)
                .ToListAsync();

            var expiredCount = expiredTokens.Count(t => t.IsExpired);
            var revokedCount = expiredTokens.Count(t => t.IsRevoked);
            var usedCount = expiredTokens.Count(t => t.IsUsed);

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();

            stopwatch.Stop();

            var result = new TokenCleanupResultDto
            {
                ExpiredTokensRemoved = expiredCount,
                RevokedTokensRemoved = revokedCount,
                UsedTokensRemoved = usedCount,
                CleanupTime = stopwatch.Elapsed,
                CleanupAt = DateTime.UtcNow,
                Message = $"Successfully cleaned up {expiredTokens.Count} tokens in {stopwatch.ElapsedMilliseconds}ms"
            };

            _logger.LogInformation("Token cleanup completed: {Result}", result.Message);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token cleanup");
            throw;
        }
    }

    public async Task<TokenValidationResultDto> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenInfo = await GetTokenInfoAsync(token);
            
            if (tokenInfo == null)
            {
                return new TokenValidationResultDto
                {
                    IsValid = false,
                    Message = "Token not found",
                    ErrorCode = "TOKEN_NOT_FOUND",
                    ValidatedAt = DateTime.UtcNow
                };
            }

            if (!tokenInfo.IsActive)
            {
                var reason = tokenInfo.IsExpired ? "expired" :
                           tokenInfo.IsRevoked ? "revoked" :
                           tokenInfo.IsUsed ? "used" : "invalid";

                return new TokenValidationResultDto
                {
                    IsValid = false,
                    Message = $"Token is {reason}",
                    ErrorCode = $"TOKEN_{reason.ToUpper()}",
                    TokenInfo = tokenInfo,
                    ValidatedAt = DateTime.UtcNow
                };
            }

            return new TokenValidationResultDto
            {
                IsValid = true,
                Message = "Token is valid",
                TokenInfo = tokenInfo,
                ValidatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return new TokenValidationResultDto
            {
                IsValid = false,
                Message = "Token validation failed",
                ErrorCode = "VALIDATION_ERROR",
                ValidatedAt = DateTime.UtcNow
            };
        }
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        return Convert.ToBase64String(randomNumber);
    }

    private (string Token, string Id, DateTime ExpiresAt) GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.Id.ToString()),
            new Claim("FullName", user.FullName ?? user.Email)
        };

        var expiresAt = DateTime.UtcNow.AddMinutes(GetTokenSettings().JwtExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, token.Id, expiresAt);
    }

    private async Task RevokeOldTokensAsync(int userId)
    {
        var maxTokens = GetTokenSettings().MaxActiveTokensPerUser;
        var now = DateTime.UtcNow;
        
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed && rt.ExpiresAt > now)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();

        if (userTokens.Count > maxTokens)
        {
            var tokensToRevoke = userTokens.Skip(maxTokens);
            
            foreach (var token in tokensToRevoke)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Revoked {Count} old tokens for user {UserId}", tokensToRevoke.Count(), userId);
        }
    }

    private TokenSettingsDto GetTokenSettings()
    {
        return new TokenSettingsDto
        {
            RefreshTokenExpirationDays = _configuration.GetValue<int>("TokenSettings:RefreshTokenExpirationDays", 7),
            JwtExpirationMinutes = _configuration.GetValue<int>("TokenSettings:JwtExpirationMinutes", 60),
            MaxActiveTokensPerUser = _configuration.GetValue<int>("TokenSettings:MaxActiveTokensPerUser", 5),
            RevokeOldTokensOnNewLogin = _configuration.GetValue<bool>("TokenSettings:RevokeOldTokensOnNewLogin", true),
            RequireUserAgentValidation = _configuration.GetValue<bool>("TokenSettings:RequireUserAgentValidation", false),
            TrackTokenUsage = _configuration.GetValue<bool>("TokenSettings:TrackTokenUsage", true)
        };
    }

    private List<TokenActivityDto> GetRecentActivity(List<RefreshToken> tokens)
    {
        var activities = new List<TokenActivityDto>();
        var now = DateTime.UtcNow;

        for (int i = 6; i >= 0; i--)
        {
            var date = now.Date.AddDays(-i);
            var dayStart = date;
            var dayEnd = date.AddDays(1);

            var dayTokens = tokens.Where(t => t.CreatedAt >= dayStart && t.CreatedAt < dayEnd).ToList();

            activities.Add(new TokenActivityDto
            {
                Date = date,
                TokensCreated = dayTokens.Count,
                TokensRevoked = dayTokens.Count(t => t.IsRevoked && t.RevokedAt >= dayStart && t.RevokedAt < dayEnd),
                TokensUsed = dayTokens.Count(t => t.IsUsed),
                ActiveTokensCount = dayTokens.Count(t => t.IsActive)
            });
        }

        return activities;
    }
}

public interface IRefreshTokenService
{
    Task<TokenPairDto> GenerateTokenPairAsync(User user);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string token, string reason = null, string revokedByIp = null);
    Task<bool> RevokeAllUserTokensAsync(int userId, string reason = null);
    Task<TokenInfoDto> GetTokenInfoAsync(string token);
    Task<TokenStatsDto> GetTokenStatsAsync();
    Task<TokenCleanupResultDto> CleanupExpiredTokensAsync();
    Task<TokenValidationResultDto> ValidateTokenAsync(string token);
}
