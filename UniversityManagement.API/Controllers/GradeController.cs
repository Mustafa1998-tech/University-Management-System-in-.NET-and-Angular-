using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Teacher,Admin")]
[ApiController]
[Route("api/[controller]")]
public class GradeController : ControllerBase
{
    private readonly GradeService _service;

    public GradeController(GradeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var grades = await _service.GetAll();
        return Ok(grades);
    }

    [HttpPost]
    public async Task<IActionResult> Add(GradeDto dto)
    {
        var grade = await _service.Add(dto);
        if (grade == null)
            return BadRequest("Failed to add grade. Student, course not found, student not enrolled, or grade already exists.");

        return Ok(grade);
    }

    [HttpPut("{studentId}/{courseId}")]
    public async Task<IActionResult> Update(int studentId, int courseId, GradeDto dto)
    {
        var grade = await _service.Update(studentId, courseId, dto);
        if (grade == null)
            return NotFound("Grade not found");

        return Ok(grade);
    }

    [HttpDelete("{studentId}/{courseId}")]
    public async Task<IActionResult> Delete(int studentId, int courseId)
    {
        var result = await _service.Delete(studentId, courseId);
        if (!result)
            return NotFound("Grade not found");

        return Ok("Grade deleted successfully");
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetGradesForStudent(int studentId)
    {
        var grades = await _service.GetGradesForStudent(studentId);
        return Ok(grades);
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetGradesForCourse(int courseId)
    {
        var grades = await _service.GetGradesForCourse(courseId);
        return Ok(grades);
    }

    [HttpGet("teacher/{teacherId}")]
    public async Task<IActionResult> GetGradesByTeacher(int teacherId)
    {
        var grades = await _service.GetGradesByTeacher(teacherId);
        return Ok(grades);
    }

    [HttpGet("statistics/student/{studentId}")]
    public async Task<IActionResult> GetStudentStatistics(int studentId)
    {
        var stats = await _service.GetStudentStatistics(studentId);
        return Ok(stats);
    }

    [HttpGet("statistics/course/{courseId}")]
    public async Task<IActionResult> GetCourseStatistics(int courseId)
    {
        var stats = await _service.GetCourseStatistics(courseId);
        return Ok(stats);
    }

    // Student-specific endpoints
    [Authorize(Roles = "Student")]
    [HttpGet("my-grades")]
    public async Task<IActionResult> GetMyGrades()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        
        // Get student ID from user ID (you might need to add a method for this)
        // For now, we'll use a placeholder approach
        var grades = await _service.GetAll(); // Placeholder - should filter by student's actual ID
        
        return Ok(grades);
    }

    // Teacher-specific endpoints
    [Authorize(Roles = "Teacher")]
    [HttpGet("my-course-grades/{courseId}")]
    public async Task<IActionResult> GetMyCourseGrades(int courseId)
    {
        // Verify teacher teaches this course (you might want to add validation)
        var grades = await _service.GetGradesForCourse(courseId);
        return Ok(grades);
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost("bulk-grade")]
    public async Task<IActionResult> AddBulkGrades(List<GradeDto> grades)
    {
        var results = new List<object>();
        
        foreach (var gradeDto in grades)
        {
            var result = await _service.Add(gradeDto);
            if (result != null)
            {
                results.Add(new { Success = true, Grade = result });
            }
            else
            {
                results.Add(new { 
                    Success = false, 
                    StudentId = gradeDto.StudentId, 
                    CourseId = gradeDto.CourseId,
                    Error = "Failed to add grade"
                });
            }
        }

        return Ok(new { Message = "Bulk grade processing completed", Results = results });
    }
}
