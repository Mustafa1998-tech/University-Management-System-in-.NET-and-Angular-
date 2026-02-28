using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UniversityManagement.API.Filters;

public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var errorResponse = new
            {
                StatusCode = 400,
                Status = "Bad Request",
                Message = "Validation failed",
                Timestamp = DateTime.UtcNow,
                Errors = errors,
                ErrorId = Guid.NewGuid().ToString()
            };

            context.Result = new BadRequestObjectResult(errorResponse);
        }
    }
}
