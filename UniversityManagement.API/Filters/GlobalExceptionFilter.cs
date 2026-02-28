using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UniversityManagement.API.Middleware;

namespace UniversityManagement.API.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "An unhandled exception occurred in controller: {Controller}", 
            context.ActionDescriptor.RouteValues["controller"]);

        var errorResponse = CreateErrorResponse(context.Exception);
        
        context.Result = new ObjectResult(errorResponse)
        {
            StatusCode = errorResponse.StatusCode
        };

        context.ExceptionHandled = true;
    }

    private ErrorResponse CreateErrorResponse(Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var message = GetUserFriendlyMessage(exception);
        var errorId = Guid.NewGuid().ToString();

        _logger.LogError("Error ID: {ErrorId} | Controller Exception: {ExceptionType} | Message: {Message}", 
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
                Source = exception.Source,
                Controller = exception.TargetSite?.DeclaringType?.Name,
                Method = exception.TargetSite?.Name
            } : null
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => (int)System.Net.HttpStatusCode.BadRequest,
            NotFoundException => (int)System.Net.HttpStatusCode.NotFound,
            BusinessException => (int)System.Net.HttpStatusCode.BadRequest,
            DuplicateResourceException => (int)System.Net.HttpStatusCode.Conflict,
            UnauthorizedAccessException => (int)System.Net.HttpStatusCode.Unauthorized,
            ArgumentNullException => (int)System.Net.HttpStatusCode.BadRequest,
            ArgumentException => (int)System.Net.HttpStatusCode.BadRequest,
            InvalidOperationException => (int)System.Net.HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)System.Net.HttpStatusCode.NotFound,
            NotImplementedException => (int)System.Net.HttpStatusCode.NotImplemented,
            TimeoutException => (int)System.Net.HttpStatusCode.RequestTimeout,
            _ => (int)System.Net.HttpStatusCode.InternalServerError
        };
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ValidationException validation => $"Validation failed: {validation.Message}",
            NotFoundException => "The requested resource was not found.",
            BusinessException business => business.Message,
            DuplicateResourceException duplicate => duplicate.Message,
            ArgumentNullException => "Required parameter is missing. Please provide all required fields.",
            ArgumentException => "Invalid input provided. Please check your request parameters.",
            InvalidOperationException => "The operation is not valid in the current state.",
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            KeyNotFoundException => "The requested resource was not found.",
            NotImplementedException => "This feature is not yet implemented.",
            TimeoutException => "The request timed out. Please try again later.",
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
