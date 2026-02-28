using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.API.Middleware;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationMiddleware> _logger;

    public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Store the original response body stream
        var originalBodyStream = context.Response.Body;

        // Create a new memory stream to capture the response
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            // Continue processing the request
            await _next(context);

            // Check if there are validation errors
            if (context.Items.ContainsKey("ValidationErrors"))
            {
                var errors = context.Items["ValidationErrors"];
                
                // Reset the response
                context.Response.Body = originalBodyStream;
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                // Create structured validation error response
                var validationResponse = new
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Status = "Bad Request",
                    Message = "Validation failed",
                    Timestamp = DateTime.UtcNow,
                    ErrorId = Guid.NewGuid().ToString(),
                    Errors = errors,
                    Path = context.Request.Path,
                    Method = context.Request.Method
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var jsonResponse = JsonSerializer.Serialize(validationResponse, jsonOptions);
                await context.Response.WriteAsync(jsonResponse);
                
                _logger.LogWarning("Validation failed for {Path} with errors: {Errors}", 
                    context.Request.Path, errors);
                
                return;
            }

            // If no validation errors, copy the response back to the original stream
            context.Response.Body = originalBodyStream;
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            // Restore the original response body stream
            context.Response.Body = originalBodyStream;
            
            // Log the error
            _logger.LogError(ex, "Error occurred in validation middleware");
            
            // Re-throw the exception to be handled by the error handling middleware
            throw;
        }
    }
}

// Enhanced Validation Filter with detailed error reporting
public class EnhancedValidationFilter : IActionFilter
{
    private readonly ILogger<EnhancedValidationFilter> _logger;

    public EnhancedValidationFilter(ILogger<EnhancedValidationFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new Dictionary<string, List<string>>();
            
            // Extract validation errors from ModelState
            foreach (var keyValuePair in context.ModelState)
            {
                if (keyValuePair.Value.Errors.Count > 0)
                {
                    var errorMessages = keyValuePair.Value.Errors
                        .Select(error => error.ErrorMessage)
                        .ToList();
                    
                    errors[keyValuePair.Key] = errorMessages;
                }
            }

            // Log validation errors
            _logger.LogWarning("Validation failed for {Controller}/{Action}. Errors: {Errors}", 
                context.ActionDescriptor.RouteValues["controller"],
                context.ActionDescriptor.RouteValues["action"],
                errors);

            // Store errors in HttpContext for middleware to handle
            context.HttpContext.Items["ValidationErrors"] = errors;
            
            // Create a result to prevent the action from executing
            context.Result = new BadRequestObjectResult(new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Validation failed",
                Timestamp = DateTime.UtcNow,
                ErrorId = Guid.NewGuid().ToString(),
                Errors = errors,
                Path = context.HttpContext.Request.Path,
                Method = context.HttpContext.Request.Method
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // This method is called after the action executes
        // We can use it for post-action validation if needed
    }
}

// Custom validation result for more detailed reporting
public class ValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public string Message { get; set; }
    public string ErrorId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ValidationResult Success()
    {
        return new ValidationResult { IsValid = true, Message = "Validation successful" };
    }

    public static ValidationResult Failure(string message, Dictionary<string, List<string>> errors = null)
    {
        return new ValidationResult 
        { 
            IsValid = false, 
            Message = message, 
            Errors = errors ?? new Dictionary<string, List<string>>() 
        };
    }

    public static ValidationResult Failure(string fieldName, string errorMessage)
    {
        return new ValidationResult 
        { 
            IsValid = false, 
            Message = "Validation failed", 
            Errors = new Dictionary<string, List<string>>
            {
                { fieldName, new List<string> { errorMessage } }
            }
        };
    }
}

// Validation helper for custom validation logic
public static class ValidationHelper
{
    public static ValidationResult ValidateStudentEmail(string email, int studentId, UniversityDbContext context)
    {
        var errors = new Dictionary<string, List<string>>();

        // Check if email is already used by another student
        var existingStudent = context.Students
            .Include(s => s.User)
            .FirstOrDefault(s => s.User.Email == email && s.Id != studentId);

        if (existingStudent != null)
        {
            errors["Email"] = new List<string> { "This email is already used by another student" };
        }

        return errors.Any() ? ValidationResult.Failure("Email validation failed", errors) : ValidationResult.Success();
    }

    public static ValidationResult ValidateCourseCapacity(int courseId, UniversityDbContext context)
    {
        var errors = new Dictionary<string, List<string>>();

        var course = context.Courses.Find(courseId);
        if (course == null)
        {
            errors["CourseId"] = new List<string> { "Course not found" };
            return ValidationResult.Failure("Course validation failed", errors);
        }

        var enrolledStudents = context.StudentCourses.Count(sc => sc.CourseId == courseId);
        var maxCapacity = 50; // This could be a property of the Course entity

        if (enrolledStudents >= maxCapacity)
        {
            errors["CourseId"] = new List<string> { $"Course has reached maximum capacity ({maxCapacity} students)" };
        }

        return errors.Any() ? ValidationResult.Failure("Course validation failed", errors) : ValidationResult.Success();
    }

    public static ValidationResult ValidateGradeRange(double score)
    {
        var errors = new Dictionary<string, List<string>>();

        if (score < 0 || score > 100)
        {
            errors["Score"] = new List<string> { "Score must be between 0 and 100" };
        }

        return errors.Any() ? ValidationResult.Failure("Grade validation failed", errors) : ValidationResult.Success();
    }

    public static ValidationResult ValidateDepartmentExists(int departmentId, UniversityDbContext context)
    {
        var errors = new Dictionary<string, List<string>>();

        var department = context.Departments.Find(departmentId);
        if (department == null)
        {
            errors["DepartmentId"] = new List<string> { "Department not found" };
        }

        return errors.Any() ? ValidationResult.Failure("Department validation failed", errors) : ValidationResult.Success();
    }
}
