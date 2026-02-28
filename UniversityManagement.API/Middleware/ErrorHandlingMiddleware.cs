using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace UniversityManagement.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        
        var errorResponse = CreateErrorResponse(exception);
        context.Response.StatusCode = errorResponse.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private ErrorResponse CreateErrorResponse(Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var message = GetUserFriendlyMessage(exception);
        var errorId = Guid.NewGuid().ToString();

        _logger.LogError("Error ID: {ErrorId} | Exception: {ExceptionType} | Message: {Message}", 
            errorId, exception.GetType().Name, exception.Message);

        return new ErrorResponse
        {
            StatusCode = statusCode,
            Status = GetStatusText(statusCode),
            Message = message,
            ErrorId = errorId,
            Timestamp = DateTime.UtcNow,
            Path = _env.IsDevelopment() ? exception.StackTrace : null,
            Details = _env.IsDevelopment() ? new
            {
                ExceptionType = exception.GetType().Name,
                InnerException = exception.InnerException?.Message,
                Source = exception.Source
            } : null
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            NotImplementedException => (int)HttpStatusCode.NotImplemented,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            HttpRequestException => (int)HttpStatusCode.BadGateway,
            TaskCanceledException => (int)HttpStatusCode.RequestTimeout,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "Required parameter is missing. Please provide all required fields.",
            ArgumentException => "Invalid input provided. Please check your request parameters.",
            InvalidOperationException => "The operation is not valid in the current state.",
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            KeyNotFoundException => "The requested resource was not found.",
            NotImplementedException => "This feature is not yet implemented.",
            TimeoutException => "The request timed out. Please try again later.",
            HttpRequestException => "There was a problem with the request. Please try again.",
            TaskCanceledException => "The request was cancelled.",
            _ => "An internal server error occurred. Please contact support if the problem persists."
        };
    }

    private static string GetStatusText(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            408 => "Request Timeout",
            409 => "Conflict",
            410 => "Gone",
            422 => "Unprocessable Entity",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => "Error"
        };
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public string ErrorId { get; set; }
    public DateTime Timestamp { get; set; }
    public object Path { get; set; }
    public object Details { get; set; }
}

// Custom exception types for better error handling
public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(string message, List<string> errors = null) : base(message)
    {
        Errors = errors ?? new List<string>();
    }
}

public class NotFoundException : Exception
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public NotFoundException(string resourceType, object resourceId) 
        : base($"{resourceType} with ID {resourceId} was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

public class BusinessException : Exception
{
    public string ErrorCode { get; }

    public BusinessException(string message, string errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class DuplicateResourceException : Exception
{
    public string ResourceType { get; }
    public object ResourceId { get; }

    public DuplicateResourceException(string resourceType, object resourceId) 
        : base($"{resourceType} with ID {resourceId} already exists.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}
