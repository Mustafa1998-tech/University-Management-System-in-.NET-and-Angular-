using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UniversityManagement.Application.Auth;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;
using UniversityManagement.Infrastructure.Services;

namespace UniversityManagement.Infrastructure.Auth;

public class AuthService
{
    private readonly UniversityDbContext _context;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IConfiguration _configuration;

    public AuthService(UniversityDbContext context, IRefreshTokenService refreshTokenService, IConfiguration configuration)
    {
        _context = context;
        _refreshTokenService = refreshTokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> Register(RegisterDto dto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (existingUser != null)
            return new AuthResponseDto { IsSuccess = false, Message = "Email already exists" };

        // Check if department exists
        var department = await _context.Departments.FindAsync(dto.DepartmentId);
        if (department == null)
            return new AuthResponseDto { IsSuccess = false, Message = "Department not found" };

        // Create user
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create role-specific entity
        if (dto.Role == "Student")
        {
            var student = new Student
            {
                FullName = dto.FullName,
                DepartmentId = dto.DepartmentId,
                UserId = user.Id
            };
            _context.Students.Add(student);
        }
        else if (dto.Role == "Teacher")
        {
            var teacher = new Teacher
            {
                FullName = dto.FullName,
                DepartmentId = dto.DepartmentId,
                UserId = user.Id
            };
            _context.Teachers.Add(teacher);
        }

        await _context.SaveChangesAsync();

        // Generate tokens
        var tokenPair = await _refreshTokenService.GenerateTokenPairAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "User registered successfully",
            Token = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            Role = user.Role,
            UserId = user.Id,
            ExpiresAt = tokenPair.AccessTokenExpiresAt
        };
    }

    public async Task<AuthResponseDto> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return new AuthResponseDto { IsSuccess = false, Message = "Invalid email or password" };

        // Generate tokens
        var tokenPair = await _refreshTokenService.GenerateTokenPairAsync(user);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Login successful",
            Token = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            Role = user.Role,
            UserId = user.Id,
            ExpiresAt = tokenPair.AccessTokenExpiresAt
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var tokenResponse = await _refreshTokenService.RefreshTokenAsync(refreshToken);
            
            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Token refreshed successfully",
                Token = tokenResponse.JwtToken,
                RefreshToken = tokenResponse.RefreshToken,
                Role = tokenResponse.Role,
                UserId = tokenResponse.UserId,
                ExpiresAt = tokenResponse.ExpiresAt
            };
        }
        catch (SecurityTokenException ex)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Failed to refresh token"
            };
        }
    }

    public async Task<bool> RevokeTokenAsync(string token, string reason = null)
    {
        return await _refreshTokenService.RevokeTokenAsync(token, reason);
    }

    public async Task<bool> RevokeAllUserTokensAsync(int userId, string reason = null)
    {
        return await _refreshTokenService.RevokeAllUserTokensAsync(userId, reason);
    }
}
