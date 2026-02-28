using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Teacher,Admin")]
[ApiController]
[Route("api/[controller]")]
public class EnrollmentController : ControllerBase
{
    private readonly EnrollmentService _service;

    public EnrollmentController(EnrollmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await _service.GetAll();
        return Ok(enrollments);
    }

    [HttpPost]
    public async Task<IActionResult> Add(EnrollmentDto dto)
    {
        var result = await _service.Add(dto);
        if (result == null)
            return BadRequest("Failed to enroll student. Student or course doesn't exist, or student is already enrolled.");

        return Ok(new { Message = "Student enrolled successfully", StudentId = dto.StudentId, CourseId = dto.CourseId });
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetCoursesForStudent(int studentId)
    {
        var enrollments = await _service.GetCoursesForStudent(studentId);
        return Ok(enrollments);
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetStudentsInCourse(int courseId)
    {
        var enrollments = await _service.GetStudentsInCourse(courseId);
        return Ok(enrollments);
    }

    [HttpDelete("{studentId}/{courseId}")]
    public async Task<IActionResult> Delete(int studentId, int courseId)
    {
        var result = await _service.Delete(studentId, courseId);
        if (!result)
            return NotFound("Enrollment not found");

        return Ok("Enrollment deleted successfully");
    }

    [HttpGet("department/{departmentId}")]
    [Authorize(Roles = "Admin")] // Only Admin can see department-wide enrollments
    public async Task<IActionResult> GetEnrollmentsByDepartment(int departmentId)
    {
        var enrollments = await _service.GetEnrollmentsByDepartment(departmentId);
        return Ok(enrollments);
    }

    [HttpGet("check/{studentId}/{courseId}")]
    public async Task<IActionResult> CheckEnrollment(int studentId, int courseId)
    {
        var isEnrolled = await _service.IsStudentEnrolled(studentId, courseId);
        return Ok(new { IsEnrolled = isEnrolled });
    }

    [HttpGet("stats/student/{studentId}")]
    public async Task<IActionResult> GetStudentCourseCount(int studentId)
    {
        var count = await _service.GetStudentCourseCount(studentId);
        return Ok(new { StudentId = studentId, CourseCount = count });
    }

    [HttpGet("stats/course/{courseId}")]
    public async Task<IActionResult> GetCourseStudentCount(int courseId)
    {
        var count = await _service.GetCourseStudentCount(courseId);
        return Ok(new { CourseId = courseId, StudentCount = count });
    }

    // Student-specific endpoints
    [Authorize(Roles = "Student")]
    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        
        // Get student ID from user ID (you might need to add a method for this)
        // For now, we'll use a placeholder approach
        var enrollments = await _service.GetAll(); // Placeholder - should filter by student's actual ID
        
        return Ok(enrollments);
    }

    // Teacher-specific endpoints
    [Authorize(Roles = "Teacher")]
    [HttpGet("my-course-students/{courseId}")]
    public async Task<IActionResult> GetMyCourseStudents(int courseId)
    {
        // Verify teacher teaches this course (you might want to add validation)
        var enrollments = await _service.GetStudentsInCourse(courseId);
        return Ok(enrollments);
    }
}
