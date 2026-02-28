using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class TeacherController : ControllerBase
{
    private readonly TeacherService _service;

    public TeacherController(TeacherService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var teachers = await _service.GetAll();
        return Ok(teachers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var teacher = await _service.GetById(id);
        if (teacher == null)
            return NotFound("Teacher not found");

        return Ok(teacher);
    }

    [HttpPost]
    public async Task<IActionResult> Add(TeacherDto dto)
    {
        var teacher = await _service.Add(dto);
        if (teacher == null)
            return BadRequest("Failed to create teacher. User may already have a teacher record or department doesn't exist.");

        return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, teacher);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TeacherDto dto)
    {
        var teacher = await _service.Update(id, dto);
        if (teacher == null)
            return NotFound("Teacher not found or department doesn't exist");

        return Ok(teacher);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.Delete(id);
        if (!result)
            return NotFound("Teacher not found");

        return Ok("Teacher deleted successfully");
    }

    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var teachers = await _service.GetByDepartment(departmentId);
        return Ok(teachers);
    }

    // Teacher-specific endpoints (for teachers themselves)
    [Authorize(Roles = "Teacher")]
    [HttpGet("my-profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var teachers = await _service.GetAll();
        var teacher = teachers.FirstOrDefault(t => t.UserId == userId);
        
        if (teacher == null)
            return NotFound("Teacher profile not found");

        return Ok(teacher);
    }

    [Authorize(Roles = "Teacher")]
    [HttpGet("my-courses")]
    public IActionResult GetMyCourses()
    {
        // Teacher can see their assigned courses
        return Ok(new { Message = "Teacher courses - Protected for Teachers only" });
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost("add-grade")]
    public IActionResult AddGrade(int studentId, int courseId, decimal grade)
    {
        // Teacher can add grades for students
        return Ok(new { Message = $"Grade {grade} added for student {studentId} in course {courseId}" });
    }

    [Authorize(Roles = "Teacher")]
    [HttpGet("students-in-course/{courseId}")]
    public IActionResult GetStudentsInCourse(int courseId)
    {
        // Teacher can see students in their courses
        return Ok(new { Message = $"Students in course {courseId} - Protected for Teachers only" });
    }
}
