using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Teacher,Admin")]
[ApiController]
[Route("api/[controller]")]
public class EnhancedStudentController : ControllerBase
{
    private readonly EnhancedStudentService _service;

    public EnhancedStudentController(EnhancedStudentService service)
    {
        _service = service;
    }

    // Enhanced Pagination with Search, Filtering, and Sorting
    [HttpGet("enhanced-paginated")]
    public async Task<IActionResult> GetEnhancedPaginated([FromQuery] AdvancedSearchParametersDto searchParams)
    {
        try
        {
            var result = await _service.GetEnhancedPaginated(searchParams);
            return Ok(ApiResponseDto<EnhancedPaginatedResponseDto<StudentResponseDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<EnhancedPaginatedResponseDto<StudentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Advanced Search
    [HttpPost("advanced-search")]
    public async Task<IActionResult> AdvancedSearch([FromBody] AdvancedSearchParametersDto searchParams)
    {
        try
        {
            var result = await _service.AdvancedSearch(searchParams);
            return Ok(ApiResponseDto<List<StudentResponseDto>>.SuccessResult(result, "Search completed successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<StudentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Quick Search (for autocomplete/typeahead)
    [HttpGet("quick-search")]
    public async Task<IActionResult> QuickSearch([FromQuery] string term, [FromQuery] int limit = 10)
    {
        try
        {
            var result = await _service.QuickSearch(term, limit);
            return Ok(ApiResponseDto<List<StudentResponseDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<StudentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Department Statistics
    [HttpGet("department-statistics/{departmentId}")]
    public async Task<IActionResult> GetDepartmentStatistics(int departmentId)
    {
        try
        {
            var result = await _service.GetDepartmentStatistics(departmentId);
            if (result == null)
                return NotFound(ApiResponseDto<DepartmentStatisticsDto>.ErrorResult("Department not found", "DEPARTMENT_NOT_FOUND"));

            return Ok(ApiResponseDto<DepartmentStatisticsDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<DepartmentStatisticsDto>.ErrorResult(ex.Message));
        }
    }

    // Bulk Operations
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreateStudents([FromBody] BulkOperationDto<StudentDto> bulkOperation)
    {
        try
        {
            var result = await _service.BulkCreateStudents(bulkOperation);
            return Ok(ApiResponseDto<BulkOperationResultDto<StudentDto>>.SuccessResult(result, 
                $"Bulk operation completed: {result.SuccessfulItems} successful, {result.FailedItems} failed"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<BulkOperationResultDto<StudentDto>>.ErrorResult(ex.Message));
        }
    }

    // Export Students
    [HttpPost("export")]
    public async Task<IActionResult> ExportStudents([FromBody] ExportParametersDto exportParams)
    {
        try
        {
            var data = await _service.ExportStudents(exportParams);
            
            var contentType = exportParams.Format.ToLower() switch
            {
                "csv" => "text/csv",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/json"
            };

            var fileName = $"students_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{exportParams.Format}";
            
            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<string>.ErrorResult(ex.Message));
        }
    }

    // Filter by Department (legacy endpoint for compatibility)
    [HttpGet("filter-by-department/{departmentId}")]
    public async Task<IActionResult> FilterByDepartment(int departmentId)
    {
        try
        {
            var searchParams = new AdvancedSearchParametersDto
            {
                DepartmentId = departmentId,
                PageSize = 1000 // Large number to get all results
            };

            var result = await _service.AdvancedSearch(searchParams);
            return Ok(ApiResponseDto<List<StudentResponseDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<StudentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Simple Pagination (legacy endpoint for compatibility)
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var searchParams = new AdvancedSearchParametersDto
            {
                Page = page,
                PageSize = pageSize
            };

            var result = await _service.GetEnhancedPaginated(searchParams);
            return Ok(ApiResponseDto<EnhancedPaginatedResponseDto<StudentResponseDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<EnhancedPaginatedResponseDto<StudentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Search (legacy endpoint for compatibility)
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        try
        {
            var result = await _service.QuickSearch(term, 50);
            return Ok(ApiResponseDto<List<StudentResponseDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseDto<List<StudentResponseDto>>.ErrorResult(ex.Message));
        }
    }

    // Health Check endpoint
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "EnhancedStudentController",
            Timestamp = DateTime.UtcNow,
            Version = "2.0.0"
        });
    }

    // API Documentation endpoint
    [HttpGet("docs")]
    [AllowAnonymous]
    public IActionResult GetApiDocumentation()
    {
        var docs = new
        {
            Title = "Enhanced Student API",
            Version = "2.0.0",
            Endpoints = new[]
            {
                new { Method = "GET", Path = "/api/enhanced-student/enhanced-paginated", Description = "Enhanced pagination with search and filtering" },
                new { Method = "POST", Path = "/api/enhanced-student/advanced-search", Description = "Advanced search with multiple criteria" },
                new { Method = "GET", Path = "/api/enhanced-student/quick-search", Description = "Quick search for autocomplete" },
                new { Method = "GET", Path = "/api/enhanced-student/department-statistics/{departmentId}", Description = "Get department statistics" },
                new { Method = "POST", Path = "/api/enhanced-student/bulk-create", Description = "Bulk create students" },
                new { Method = "POST", Path = "/api/enhanced-student/export", Description = "Export students data" }
            },
            Parameters = new
            {
                AdvancedSearchParameters = new
                {
                    Query = "string - Search term",
                    DepartmentId = "int? - Filter by department",
                    Page = "int - Page number (default: 1)",
                    PageSize = "int - Items per page (default: 10)",
                    SortBy = "string - Sort field (default: Id)",
                    SortDescending = "bool - Sort direction (default: false)"
                }
            },
            Examples = new[]
            {
                new
                {
                    Description = "Get paginated students with search",
                    Url = "/api/enhanced-student/enhanced-paginated?query=ahmed&page=1&pageSize=10&sortBy=name&sortDescending=false"
                },
                new
                {
                    Description = "Quick search",
                    Url = "/api/enhanced-student/quick-search?term=computer&limit=5"
                },
                new
                {
                    Description = "Department statistics",
                    Url = "/api/enhanced-student/department-statistics/1"
                }
            }
        };

        return Ok(docs);
    }
}
