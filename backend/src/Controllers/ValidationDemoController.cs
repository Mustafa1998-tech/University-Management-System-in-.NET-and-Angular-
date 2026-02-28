using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.API.Middleware;

[ApiController]
[Route("api/[controller]")]
public class ValidationDemoController : ControllerBase
{
    // Test student validation
    [HttpPost("test-student-validation")]
    public IActionResult TestStudentValidation([FromBody] StudentDto dto)
    {
        return Ok(new { Message = "Student validation passed", Data = dto });
    }

    // Test teacher validation
    [HttpPost("test-teacher-validation")]
    public IActionResult TestTeacherValidation([FromBody] TeacherDto dto)
    {
        return Ok(new { Message = "Teacher validation passed", Data = dto });
    }

    // Test course validation
    [HttpPost("test-course-validation")]
    public IActionResult TestCourseValidation([FromBody] CourseDto dto)
    {
        return Ok(new { Message = "Course validation passed", Data = dto });
    }

    // Test grade validation
    [HttpPost("test-grade-validation")]
    public IActionResult TestGradeValidation([FromBody] GradeDto dto)
    {
        return Ok(new { Message = "Grade validation passed", Data = dto });
    }

    // Test registration validation
    [HttpPost("test-registration-validation")]
    public IActionResult TestRegistrationValidation([FromBody] RegisterDto dto)
    {
        return Ok(new { Message = "Registration validation passed", Data = dto });
    }

    // Test login validation
    [HttpPost("test-login-validation")]
    public IActionResult TestLoginValidation([FromBody] LoginDto dto)
    {
        return Ok(new { Message = "Login validation passed", Data = dto });
    }

    // Test search parameters validation
    [HttpGet("test-search-validation")]
    public IActionResult TestSearchValidation([FromQuery] SearchParametersDto parameters)
    {
        return Ok(new { Message = "Search validation passed", Data = parameters });
    }

    // Test advanced search validation
    [HttpPost("test-advanced-search-validation")]
    public IActionResult TestAdvancedSearchValidation([FromBody] AdvancedSearchParametersDto parameters)
    {
        return Ok(new { Message = "Advanced search validation passed", Data = parameters });
    }

    // Test bulk operation validation
    [HttpPost("test-bulk-validation")]
    public IActionResult TestBulkValidation([FromBody] BulkCreateStudentsDto bulkDto)
    {
        return Ok(new { Message = "Bulk validation passed", Data = new { StudentCount = bulkDto.Students.Count } });
    }

    // Test validation with missing required fields
    [HttpPost("test-missing-fields")]
    public IActionResult TestMissingFields([FromBody] Dictionary<string, object> data)
    {
        return Ok(new { Message = "Request processed", Data = data });
    }

    // Test validation with invalid data
    [HttpPost("test-invalid-data")]
    public IActionResult TestInvalidData([FromBody] Dictionary<string, object> data)
    {
        return Ok(new { Message = "Request processed", Data = data });
    }

    // Get validation examples
    [HttpGet("validation-examples")]
    [AllowAnonymous]
    public IActionResult GetValidationExamples()
    {
        var examples = new
        {
            Title = "Validation Examples",
            Description = "Examples of validation rules and error responses",
            ValidationRules = new
            {
                StudentValidation = new
                {
                    Rules = new[]
                    {
                        "FullName: Required, MinLength(3), MaxLength(100), Letters only",
                        "DepartmentId: Required, Range(1, max)",
                        "UserId: Required, Range(1, max)"
                    },
                    ValidExample = new
                    {
                        FullName = "John Doe",
                        DepartmentId = 1,
                        UserId = 1
                    },
                    InvalidExample = new
                    {
                        FullName = "A", // Too short
                        DepartmentId = 0, // Invalid range
                        UserId = -1  // Invalid range
                    }
                },
                GradeValidation = new
                {
                    Rules = new[]
                    {
                        "StudentId: Required, Range(1, max)",
                        "CourseId: Required, Range(1, max)",
                        "Score: Required, Range(0, 100), Max 2 decimal places"
                    },
                    ValidExample = new
                    {
                        StudentId = 1,
                        CourseId = 1,
                        Score = 85.5
                    },
                    InvalidExample = new
                    {
                        StudentId = 0,    // Invalid range
                        CourseId = -1,   // Invalid range
                        Score = 150     // Invalid range
                    }
                },
                RegistrationValidation = new
                {
                    Rules = new[]
                    {
                        "Email: Required, EmailAddress, MaxLength(100)",
                        "Password: Required, MinLength(8), MaxLength(100), Complex pattern",
                        "Role: Required, Must be Student/Teacher/Admin",
                        "FullName: Required, MinLength(3), MaxLength(100), Letters only",
                        "DepartmentId: Required, Range(1, max)"
                    },
                    ValidExample = new
                    {
                        Email = "student@example.com",
                        Password = "SecurePass123!",
                        Role = "Student",
                        FullName = "John Doe",
                        DepartmentId = 1
                    },
                    InvalidExample = new
                    {
                        Email = "invalid-email",    // Invalid format
                        Password = "123",           // Too short
                        Role = "InvalidRole",       // Invalid role
                        FullName = "A",              // Too short
                        DepartmentId = 0           // Invalid range
                    }
                }
            },
            ErrorResponses = new
            {
                ValidationError = new
                {
                    StatusCode = 400,
                    Status = "Bad Request",
                    Message = "Validation failed",
                    Timestamp = "2026-02-24T03:56:00Z",
                    ErrorId = "guid-example-123",
                    Errors = new
                    {
                        FullName = new[] { "Full name must be at least 3 characters" },
                        Email = new[] { "Please enter a valid email address" }
                    },
                    Path = "/api/validationdemo/test-student-validation",
                    Method = "POST"
                }
            },
            TestEndpoints = new[]
            {
                new
                {
                    Description = "Test student validation with valid data",
                    Url = "/api/validationdemo/test-student-validation",
                    Method = "POST",
                    ValidBody = new { FullName = "John Doe", DepartmentId = 1, UserId = 1 }
                },
                new
                {
                    Description = "Test student validation with invalid data",
                    Url = "/api/validationdemo/test-student-validation",
                    Method = "POST",
                    InvalidBody = new { FullName = "A", DepartmentId = 0, UserId = -1 }
                },
                new
                {
                    Description = "Test registration validation",
                    Url = "/api/validationdemo/test-registration-validation",
                    Method = "POST"
                },
                new
                {
                    Description = "Test grade validation",
                    Url = "/api/validationdemo/test-grade-validation",
                    Method = "POST"
                },
                new
                {
                    Description = "Test search parameters validation",
                    Url = "/api/validationdemo/test-search-validation?query=test&page=1",
                    Method = "GET"
                }
            }
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
            Service = "ValidationDemoController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "Data Annotations Validation",
                "Custom Validation Attributes",
                "Model State Validation",
                "Validation Middleware",
                "Enhanced Error Responses",
                "Structured Error Reporting"
            }
        });
    }
}
