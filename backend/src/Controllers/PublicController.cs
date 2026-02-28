using Microsoft.AspNetCore.Mvc;

// No [Authorize] attribute - public endpoints
[ApiController]
[Route("api/[controller]")]
public class PublicController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        // Public endpoint - no authentication required
        return Ok(new { Status = "Healthy", Message = "API is running" });
    }

    [HttpGet("departments")]
    public IActionResult GetPublicDepartments()
    {
        // Public endpoint - anyone can see departments
        return Ok(new { Message = "Public departments list - No authentication required" });
    }
}
