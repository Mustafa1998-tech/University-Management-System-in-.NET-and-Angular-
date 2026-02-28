using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize] // Any authenticated user can access
[ApiController]
[Route("api/[controller]")]
public class SharedController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        // Any authenticated user can see their profile
        var userId = User.FindFirst("UserId")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new { 
            UserId = userId,
            Email = email,
            Role = role,
            Message = "User profile - Protected for any authenticated user"
        });
    }

    [HttpGet("departments")]
    public IActionResult GetDepartments()
    {
        // Any authenticated user can see departments
        return Ok(new { Message = "Departments list - Protected for any authenticated user" });
    }

    [Authorize(Roles = "Teacher,Admin")] // Multiple roles
    [HttpPost("create-course")]
    public IActionResult CreateCourse(string name, int departmentId)
    {
        // Only Teacher or Admin can create courses
        return Ok(new { Message = $"Course '{name}' created - Protected for Teacher and Admin" });
    }
}
