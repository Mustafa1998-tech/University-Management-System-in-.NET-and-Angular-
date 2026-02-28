using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UniversityManagement.Application.Auth;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _auth.Register(dto);
        if (!result.IsSuccess) 
            return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _auth.Login(dto);
        if (!result.IsSuccess) 
            return BadRequest(result.Message);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _auth.RefreshTokenAsync(request.Token);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(new { Message = result.Message });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to refresh token" });
        }
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _auth.RevokeTokenAsync(request.Token, "User logout");
            
            if (result)
            {
                return Ok(new { Message = "Token revoked successfully" });
            }
            else
            {
                return NotFound(new { Message = "Token not found" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to revoke token" });
        }
    }

    [HttpPost("revoke-all")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _auth.RevokeAllUserTokensAsync(userId, "User logout from all devices");
            
            if (result)
            {
                return Ok(new { Message = "All tokens revoked successfully" });
            }
            else
            {
                return StatusCode(500, new { Message = "Failed to revoke tokens" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Failed to revoke tokens" });
        }
    }
}
