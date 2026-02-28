using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("all-users")]
    public IActionResult GetAllUsers()
    {
        // Admin can see all users
        return Ok(new { Message = "All users - Protected for Admin only" });
    }

    [HttpPost("create-department")]
    public IActionResult CreateDepartment(string name)
    {
        // Admin can create departments
        return Ok(new { Message = $"Department '{name}' created - Protected for Admin only" });
    }

    [HttpDelete("delete-user/{userId}")]
    public IActionResult DeleteUser(int userId)
    {
        // Admin can delete users
        return Ok(new { Message = $"User {userId} deleted - Protected for Admin only" });
    }
}
