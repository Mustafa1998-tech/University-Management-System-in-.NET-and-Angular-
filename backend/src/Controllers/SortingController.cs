using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Teacher,Admin")]
[ApiController]
[Route("api/[controller]")]
public class SortingController : ControllerBase
{
    private readonly SortingService _service;

    public SortingController(SortingService service)
    {
        _service = service;
    }

    // Student sorting endpoints
    [HttpGet("students")]
    public async Task<IActionResult> GetSortedStudents([FromQuery] string sortBy = "name", [FromQuery] string sortDirection = "asc")
    {
        try
        {
            var result = await _service.GetSortedStudents(sortBy, sortDirection);
            return Ok(ApiResponseDto<List<StudentResponseDto>>.SuccessResult(result, $"Students sorted by {sortBy} ({sortDirection})"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<StudentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    [HttpGet("students/multi-sort")]
    public async Task<IActionResult> GetMultiSortedStudents(
        [FromQuery] string primarySort = "name", 
        [FromQuery] string primaryDirection = "asc",
        [FromQuery] string secondarySort = "email", 
        [FromQuery] string secondaryDirection = "asc")
    {
        try
        {
            var result = await _service.GetMultiSortedStudents(primarySort, primaryDirection, secondarySort, secondaryDirection);
            return Ok(ApiResponseDto<List<StudentResponseDto>>.SuccessResult(result, 
                $"Students multi-sorted: {primarySort} ({primaryDirection}), then {secondarySort} ({secondaryDirection})"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<StudentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Teacher sorting endpoints
    [HttpGet("teachers")]
    public async Task<IActionResult> GetSortedTeachers([FromQuery] string sortBy = "name", [FromQuery] string sortDirection = "asc")
    {
        try
        {
            var result = await _service.GetSortedTeachers(sortBy, sortDirection);
            return Ok(ApiResponseDto<List<TeacherResponseDto>>.SuccessResult(result, $"Teachers sorted by {sortBy} ({sortDirection})"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<TeacherResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Course sorting endpoints
    [HttpGet("courses")]
    public async Task<IActionResult> GetSortedCourses([FromQuery] string sortBy = "name", [FromQuery] string sortDirection = "asc")
    {
        try
        {
            var result = await _service.GetSortedCourses(sortBy, sortDirection);
            return Ok(ApiResponseDto<List<CourseResponseDto>>.SuccessResult(result, $"Courses sorted by {sortBy} ({sortDirection})"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<CourseResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Grade sorting endpoints
    [HttpGet("grades")]
    public async Task<IActionResult> GetSortedGrades([FromQuery] string sortBy = "score", [FromQuery] string sortDirection = "desc")
    {
        try
        {
            var result = await _service.GetSortedGrades(sortBy, sortDirection);
            return Ok(ApiResponseDto<List<GradeResponseDto>>.SuccessResult(result, $"Grades sorted by {sortBy} ({sortDirection})"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<GradeResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Department sorting endpoints
    [HttpGet("departments")]
    public async Task<IActionResult> GetSortedDepartments([FromQuery] string sortBy = "name", [FromQuery] string sortDirection = "asc")
    {
        try
        {
            var result = await _service.GetSortedDepartments(sortBy, sortDirection);
            return Ok(ApiResponseDto<List<DepartmentResponseDto>>.SuccessResult(result, $"Departments sorted by {sortBy} ({sortDirection})"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<DepartmentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Sorting metadata endpoint (for UI)
    [HttpGet("metadata/{entityType}")]
    public IActionResult GetSortingMetadata(string entityType)
    {
        try
        {
            var metadata = _service.GetSortingMetadata(entityType);
            return Ok(ApiResponseDto<object>.SuccessResult(metadata, $"Sorting metadata for {entityType}"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
    }

    // All sorting metadata endpoint
    [HttpGet("metadata")]
    public IActionResult GetAllSortingMetadata()
    {
        try
        {
            var metadata = new
            {
                Student = _service.GetSortingMetadata("student"),
                Teacher = _service.GetSortingMetadata("teacher"),
                Course = _service.GetSortingMetadata("course"),
                Grade = _service.GetSortingMetadata("grade"),
                Department = _service.GetSortingMetadata("department")
            };

            return Ok(ApiResponseDto<object>.SuccessResult(metadata, "All sorting metadata"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<object>.ErrorResult(ex.Message));
        }
    }

    // Demo endpoint - show sorting examples
    [HttpGet("demo")]
    [AllowAnonymous]
    public IActionResult GetSortingDemo()
    {
        var examples = new
        {
            Title = "Sorting API Demo",
            Description = "Demonstrates various sorting capabilities",
            Examples = new[]
            {
                new
                {
                    Description = "Sort students by name (ascending)",
                    Url = "/api/sorting/students?sortBy=name&sortDirection=asc",
                    Method = "GET"
                },
                new
                {
                    Description = "Sort students by name (descending)",
                    Url = "/api/sorting/students?sortBy=name&sortDirection=desc",
                    Method = "GET"
                },
                new
                {
                    Description = "Sort students by average grade (highest first)",
                    Url = "/api/sorting/students?sortBy=averagegrade&sortDirection=desc",
                    Method = "GET"
                },
                new
                {
                    Description = "Sort teachers by number of courses",
                    Url = "/api/sorting/teachers?sortBy=coursescount&sortDirection=desc",
                    Method = "GET"
                },
                new
                {
                    Description = "Sort courses by number of students",
                    Url = "/api/sorting/courses?sortBy=studentscount&sortDirection=desc",
                    Method = "GET"
                },
                new
                {
                    Description = "Sort grades by score (highest first)",
                    Url = "/api/sorting/grades?sortBy=score&sortDirection=desc",
                    Method = "GET"
                },
                new
                {
                    Description = "Sort departments by number of students",
                    Url = "/api/sorting/departments?sortBy=studentscount&sortDirection=desc",
                    Method = "GET"
                },
                new
                {
                    Description = "Multi-level sorting: by name then email",
                    Url = "/api/sorting/students/multi-sort?primarySort=name&primaryDirection=asc&secondarySort=email&secondaryDirection=asc",
                    Method = "GET"
                },
                new
                {
                    Description = "Get sorting metadata for students",
                    Url = "/api/sorting/metadata/student",
                    Method = "GET"
                },
                new
                {
                    Description = "Get all sorting metadata",
                    Url = "/api/sorting/metadata",
                    Method = "GET"
                }
            },
            AvailableSortFields = new
            {
                Students = new[] { "id", "name", "email", "department", "enrolledcourses", "averagegrade" },
                Teachers = new[] { "id", "name", "email", "department", "coursescount" },
                Courses = new[] { "id", "name", "department", "teacher", "studentscount", "averagegrade" },
                Grades = new[] { "score", "student", "course", "department", "teacher" },
                Departments = new[] { "id", "name", "studentscount", "teacherscount", "coursescount" }
            },
            SortDirections = new[] { "asc", "desc" }
        };

        return Ok(examples);
    }

    // Health check
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "SortingController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "Single-field sorting",
                "Multi-level sorting",
                "Ascending/Descending order",
                "Sorting metadata",
                "Full DTO mapping",
                "Performance optimized"
            }
        });
    }
}
