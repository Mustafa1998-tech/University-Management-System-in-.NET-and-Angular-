namespace UniversityManagement.Application.DTOs;

// Standard API Response wrapper
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public DateTime Timestamp { get; set; }
    public string ErrorCode { get; set; }

    public static ApiResponseDto<T> SuccessResult(T data, string message = null)
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message ?? "Operation completed successfully",
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponseDto<T> ErrorResult(string message, string errorCode = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            Timestamp = DateTime.UtcNow
        };
    }
}

// Enhanced Pagination Response
public class EnhancedPaginatedResponseDto<T>
{
    public List<T> Data { get; set; }
    public PaginationMetadataDto Pagination { get; set; }
    public FilterMetadataDto Filters { get; set; }
    public SortMetadataDto Sorting { get; set; }
    public DateTime Timestamp { get; set; }
}

public class PaginationMetadataDto
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public int FirstItemIndex { get; set; }
    public int LastItemIndex { get; set; }
}

public class FilterMetadataDto
{
    public string Query { get; set; }
    public int? DepartmentId { get; set; }
    public int? TeacherId { get; set; }
    public string Role { get; set; }
    public bool HasFilters => !string.IsNullOrEmpty(Query) || DepartmentId.HasValue || TeacherId.HasValue || !string.IsNullOrEmpty(Role);
}

public class SortMetadataDto
{
    public string Field { get; set; }
    public string Direction { get; set; } // "asc" or "desc"
    public bool IsDescending => Direction.ToLower() == "desc";
}

// Advanced Search Parameters
public class AdvancedSearchParametersDto
{
    public string Query { get; set; }
    public int? DepartmentId { get; set; }
    public int? TeacherId { get; set; }
    public string Role { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "Id";
    public bool SortDescending { get; set; } = false;
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public double? MinGrade { get; set; }
    public double? MaxGrade { get; set; }
    public bool IncludeInactive { get; set; } = false;
}

// Bulk Operations DTOs
public class BulkOperationDto<T>
{
    public List<T> Items { get; set; }
    public bool ValidateBeforeExecute { get; set; } = true;
    public bool StopOnError { get; set; } = false;
}

public class BulkOperationResultDto<T>
{
    public int TotalItems { get; set; }
    public int SuccessfulItems { get; set; }
    public int FailedItems { get; set; }
    public List<BulkOperationItemResultDto<T>> Results { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}

public class BulkOperationItemResultDto<T>
{
    public T Item { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
    public string ErrorCode { get; set; }
}

// Export/Import DTOs
public class ExportParametersDto
{
    public string Format { get; set; } = "json"; // json, csv, excel
    public List<string> Fields { get; set; }
    public AdvancedSearchParametersDto Filters { get; set; }
    public bool IncludeMetadata { get; set; } = true;
}

public class ImportResultDto
{
    public int TotalRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<ImportErrorDto> Errors { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class ImportErrorDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; }
    public string Value { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
}
