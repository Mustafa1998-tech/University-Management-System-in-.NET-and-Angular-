using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.API.Middleware;
using UniversityManagement.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ErrorDemoController : ControllerBase
{
    // Test validation errors
    [HttpPost("test-validation")]
    public IActionResult TestValidation([FromBody] TestDto dto)
    {
        return Ok(new { Message = "Validation passed", Data = dto });
    }

    // Test different exception types
    [HttpGet("test-exceptions")]
    public IActionResult TestExceptions([FromQuery] string type = "general")
    {
        return type.ToLower() switch
        {
            "null" => throw new NullReferenceException("This is a null reference exception"),
            "argument" => throw new ArgumentException("This is an argument exception"),
            "notfound" => throw new NotFoundException("Student", 999),
            "business" => throw new BusinessException("This is a business logic error", "BUSINESS_ERROR"),
            "duplicate" => throw new DuplicateResourceException("Student", 1),
            "validation" => throw new ValidationException("Validation failed", new List<string> { "Name is required", "Email is invalid" }),
            "unauthorized" => throw new UnauthorizedAccessException("You are not authorized"),
            "notimplemented" => throw new NotImplementedException("This feature is not implemented"),
            "timeout" => throw new TimeoutException("Request timed out"),
            _ => throw new Exception("This is a general exception")
        };
    }

    // Test successful response
    [HttpGet("test-success")]
    public IActionResult TestSuccess()
    {
        return Ok(ApiResponseDto<object>.SuccessResult(new { Message = "Success", Timestamp = DateTime.UtcNow }));
    }

    // Test model binding error
    [HttpGet("test-model-binding/{id:int}")]
    public IActionResult TestModelBinding(int id)
    {
        return Ok(new { Id = id, Message = "Model binding successful" });
    }

    // Health check
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "ErrorDemoController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "Global Error Handling",
                "Model Validation",
                "Custom Exception Types",
                "Structured Error Responses",
                "Development vs Production Mode"
            }
        });
    }

    // Test error response format
    [HttpGet("error-response-examples")]
    [AllowAnonymous]
    public IActionResult GetErrorResponseExamples()
    {
        var examples = new
        {
            Title = "Error Response Examples",
            Description = "Different error response formats based on exception types",
            Examples = new
            {
                ValidationError = new
                {
                    StatusCode = 400,
                    Status = "Bad Request",
                    Message = "Validation failed",
                    Timestamp = "2026-02-24T03:56:00Z",
                    Errors = new[] { "Name is required", "Email is invalid" },
                    ErrorId = "guid-example-123"
                },
                NotFoundError = new
                {
                    StatusCode = 404,
                    Status = "Not Found",
                    Message = "The requested resource was not found.",
                    Timestamp = "2026-02-24T03:56:00Z",
                    ErrorId = "guid-example-456"
                },
                BusinessError = new
                {
                    StatusCode = 400,
                    Status = "Bad Request",
                    Message = "This is a business logic error",
                    Timestamp = "2026-02-24T03:56:00Z",
                    ErrorId = "guid-example-789"
                },
                InternalServerError = new
                {
                    StatusCode = 500,
                    Status = "Internal Server Error",
                    Message = "An internal server error occurred. Please contact support if the problem persists.",
                    Timestamp = "2026-02-24T03:56:00Z",
                    ErrorId = "guid-example-012"
                }
            },
            TestUrls = new[]
            {
                new { Description = "Validation Error", Url = "/api/errordemo/test-validation", Method = "POST", Body = "{}" },
                new { Description = "Null Reference Exception", Url = "/api/errordemo/test-exceptions?type=null", Method = "GET" },
                new { Description = "Not Found Exception", Url = "/api/errordemo/test-exceptions?type=notfound", Method = "GET" },
                new { Description = "Business Exception", Url = "/api/errordemo/test-exceptions?type=business", Method = "GET" },
                new { Description = "Model Binding Error", Url = "/api/errordemo/test-model-binding/abc", Method = "GET" }
            }
        };

        return Ok(examples);
    }
}

public class TestDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}
