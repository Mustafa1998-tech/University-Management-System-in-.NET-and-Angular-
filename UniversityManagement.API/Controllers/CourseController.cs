using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Admin,Teacher")]
[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly CourseService _service;

    public CourseController(CourseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var courses = await _service.GetAll();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var course = await _service.GetById(id);
        if (course == null)
            return NotFound("Course not found");

        return Ok(course);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // Only Admin can create courses
    public async Task<IActionResult> Add(CourseDto dto)
    {
        var course = await _service.Add(dto);
        if (course == null)
            return BadRequest("Failed to create course. Department or Teacher doesn't exist.");

        return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can update courses
    public async Task<IActionResult> Update(int id, CourseDto dto)
    {
        var course = await _service.Update(id, dto);
        if (course == null)
            return NotFound("Course not found or Department/Teacher doesn't exist");

        return Ok(course);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Only Admin can delete courses
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.Delete(id);
        if (!result)
            return NotFound("Course not found");

        return Ok("Course deleted successfully");
    }

    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var courses = await _service.GetByDepartment(departmentId);
        return Ok(courses);
    }

    [HttpGet("teacher/{teacherId}")]
    public async Task<IActionResult> GetByTeacher(int teacherId)
    {
        var courses = await _service.GetByTeacher(teacherId);
        return Ok(courses);
    }

    // Student-specific endpoints
    [Authorize(Roles = "Student")]
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableCourses()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        
        // Get student ID from user ID
        var student = await _service.GetAll(); // We'll need to modify this to get student by user ID
        // For now, return empty list as placeholder
        return Ok(new List<CourseResponseDto>());
    }

    // Teacher-specific endpoints
    [Authorize(Roles = "Teacher")]
    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        
        // Get teacher ID from user ID
        var courses = await _service.GetAll(); // We'll need to modify this to get teacher by user ID
        // For now, return all courses as placeholder
        return Ok(courses);
    }
}
