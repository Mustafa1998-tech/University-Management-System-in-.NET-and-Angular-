using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Teacher,Admin")]
[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly StudentService _service;

    public StudentController(StudentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var students = await _service.GetAll();
        return Ok(students);
    }

    [HttpGet("paginated")]
    public async Task<IActionResult> GetPaginated([FromQuery] SearchParametersDto searchParams)
    {
        var result = await _service.GetPaginated(searchParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var student = await _service.GetById(id);
        if (student == null) 
            return NotFound("Student not found");

        return Ok(student);
    }

    [HttpPost]
    public async Task<IActionResult> Add(StudentDto dto)
    {
        var student = await _service.Add(dto);
        if (student == null)
            return BadRequest("Failed to create student. User may already have a student record or department doesn't exist.");

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, StudentDto dto)
    {
        var student = await _service.Update(id, dto);
        if (student == null)
            return NotFound("Student not found or department doesn't exist");

        return Ok(student);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.Delete(id);
        if (!result)
            return NotFound("Student not found");

        return Ok("Student deleted successfully");
    }

    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
    {
        var students = await _service.GetByDepartment(departmentId);
        return Ok(students);
    }

    // Student-specific endpoints (for students themselves)
    [Authorize(Roles = "Student")]
    [HttpGet("my-profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var students = await _service.GetAll();
        var student = students.FirstOrDefault(s => s.UserId == userId);
        
        if (student == null)
            return NotFound("Student profile not found");

        return Ok(student);
    }

    [Authorize(Roles = "Student")]
    [HttpGet("my-courses")]
    public IActionResult GetMyCourses()
    {
        return Ok(new { Message = "Student courses - Protected for Students only" });
    }

    [Authorize(Roles = "Student")]
    [HttpGet("my-grades")]
    public IActionResult GetMyGrades()
    {
        return Ok(new { Message = "Student grades - Protected for Students only" });
    }

    // Search endpoint
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchParametersDto searchParams)
    {
        var result = await _service.GetPaginated(searchParams);
        return Ok(result);
    }
}
