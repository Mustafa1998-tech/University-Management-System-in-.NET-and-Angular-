using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class CachedStudentService : IStudentService
{
    private readonly UniversityDbContext _context;
    private readonly ILogger<CachedStudentService> _logger;
    private readonly ICacheService _cacheService;
    private readonly StudentService _baseStudentService;

    public CachedStudentService(
        UniversityDbContext context, 
        ILogger<CachedStudentService> logger,
        ICacheService cacheService,
        StudentService baseStudentService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
        _baseStudentService = baseStudentService;
    }

    public async Task<List<StudentResponseDto>> GetAll()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.STUDENTS_LIST,
            async () =>
            {
                _logger.LogDebug("Getting students list from database");
                return await _baseStudentService.GetAll();
            },
            CacheService.Expiration.Lists
        );
    }

    public async Task<StudentResponseDto> GetById(int id)
    {
        var cacheKey = $"student:by_id:{id}";
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Getting student by ID {StudentId} from database", id);
                return await _baseStudentService.GetById(id);
            },
            CacheService.Expiration.Lists
        );
    }

    public async Task<StudentResponseDto> Add(StudentDto dto)
    {
        var result = await _baseStudentService.Add(dto);
        
        if (result != null)
        {
            // Invalidate relevant caches
            _cacheService.InvalidateStudentCache();
            _cacheService.Remove($"student:by_id:{result.Id}");
            
            _logger.LogInformation("Student caches invalidated after adding student {StudentId}", result.Id);
        }
        
        return result;
    }

    public async Task<StudentResponseDto> Update(int id, StudentDto dto)
    {
        var result = await _baseStudentService.Update(id, dto);
        
        if (result != null)
        {
            // Invalidate relevant caches
            _cacheService.InvalidateStudentCache();
            _cacheService.Remove($"student:by_id:{id}");
            
            _logger.LogInformation("Student caches invalidated after updating student {StudentId}", id);
        }
        
        return result;
    }

    public async Task<bool> Delete(int id)
    {
        var result = await _baseStudentService.Delete(id);
        
        if (result)
        {
            // Invalidate relevant caches
            _cacheService.InvalidateStudentCache();
            _cacheService.Remove($"student:by_id:{id}");
            
            _logger.LogInformation("Student caches invalidated after deleting student {StudentId}", id);
        }
        
        return result;
    }

    public async Task<List<StudentResponseDto>> GetByDepartment(int departmentId)
    {
        var cacheKey = string.Format(CacheService.Keys.STUDENTS_BY_DEPARTMENT, departmentId);
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Getting students by department {DepartmentId} from database", departmentId);
                return await _baseStudentService.GetByDepartment(departmentId);
            },
            CacheService.Expiration.Lists
        );
    }

    public async Task<PaginatedResponseDto<StudentResponseDto>> GetPaginated(SearchParametersDto searchParams)
    {
        // For paginated results, we'll use shorter cache time as they're more dynamic
        var cacheKey = $"students:paginated:{searchParams.Page}:{searchParams.PageSize}:{searchParams.Query}:{searchParams.SortBy}:{searchParams.SortDescending}";
        
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Getting paginated students from database");
                return await _baseStudentService.GetPaginated(searchParams);
            },
            TimeSpan.FromMinutes(2) // Shorter cache for paginated results
        );
    }

    public async Task<List<StudentResponseDto>> QuickSearch(string searchTerm, int limit = 10)
    {
        var cacheKey = $"students:search:{searchTerm}:{limit}";
        return await _cacheService.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogDebug("Quick search students from database");
                return await _baseStudentService.QuickSearch(searchTerm, limit);
            },
            TimeSpan.FromMinutes(5) // Medium cache for search results
        );
    }

    // Cache management methods
    public void InvalidateStudentCache(int? studentId = null)
    {
        if (studentId.HasValue)
        {
            _cacheService.Remove($"student:by_id:{studentId.Value}");
            _logger.LogInformation("Cache invalidated for student {StudentId}", studentId.Value);
        }
        else
        {
            _cacheService.InvalidateStudentCache();
            _logger.LogInformation("All student caches invalidated");
        }
    }

    public async Task WarmUpStudentCache()
    {
        _logger.LogInformation("Starting student cache warm-up");
        
        try
        {
            // Warm up student list
            await GetAll();
            
            // Warm up department-wise students
            var departments = await _context.Departments.ToListAsync();
            foreach (var department in departments)
            {
                await GetByDepartment(department.Id);
            }
            
            _logger.LogInformation("Student cache warm-up completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Student cache warm-up failed");
        }
    }

    public async Task<CachePerformanceDto> GetCachePerformance()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Test cache performance
        var cachedResult = await GetAll();
        var cacheTime = stopwatch.ElapsedMilliseconds;
        
        stopwatch.Restart();
        
        // Invalidate cache and test database performance
        _cacheService.Remove(CacheService.Keys.STUDENTS_LIST);
        var dbResult = await GetAll();
        var dbTime = stopwatch.ElapsedMilliseconds;
        
        stopwatch.Stop();

        return new CachePerformanceDto
        {
            CacheResponseTime = cacheTime,
            DatabaseResponseTime = dbTime,
            PerformanceImprovement = dbTime > 0 ? (double)(dbTime - cacheTime) / dbTime * 100 : 0,
            CacheHitRate = _cacheService.GetStatistics().HitRate,
            MemoryUsage = _cacheService.GetStatistics().MemoryUsage,
            LastUpdated = DateTime.UtcNow
        };
    }
}

// Interface for Student Service to support dependency injection
public interface IStudentService
{
    Task<List<StudentResponseDto>> GetAll();
    Task<StudentResponseDto> GetById(int id);
    Task<StudentResponseDto> Add(StudentDto dto);
    Task<StudentResponseDto> Update(int id, StudentDto dto);
    Task<bool> Delete(int id);
    Task<List<StudentResponseDto>> GetByDepartment(int departmentId);
    Task<PaginatedResponseDto<StudentResponseDto>> GetPaginated(SearchParametersDto searchParams);
    Task<List<StudentResponseDto>> QuickSearch(string searchTerm, int limit = 10);
}
