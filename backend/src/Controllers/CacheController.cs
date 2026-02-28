using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly IDashboardService _dashboardService;
    private readonly IStudentService _studentService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(
        ICacheService cacheService,
        IDashboardService dashboardService,
        IStudentService studentService,
        ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _dashboardService = dashboardService;
        _studentService = studentService;
        _logger = logger;
    }

    // Get cache statistics
    [HttpGet("statistics")]
    public IActionResult GetCacheStatistics()
    {
        try
        {
            var stats = _cacheService.GetStatistics();
            return Ok(ApiResponseDto<CacheStatistics>.SuccessResult(stats, "Cache statistics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache statistics");
            return StatusCode(500, ApiResponseDto<CacheStatistics>.ErrorResult("Failed to retrieve cache statistics"));
        }
    }

    // Clear all cache
    [HttpPost("clear")]
    public IActionResult ClearAllCache()
    {
        try
        {
            _cacheService.ClearAll();
            return Ok(ApiResponseDto<string>.SuccessResult("All cache cleared successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to clear cache"));
        }
    }

    // Clear cache by pattern
    [HttpPost("clear/{pattern}")]
    public IActionResult ClearCacheByPattern(string pattern)
    {
        try
        {
            _cacheService.RemoveByPattern(pattern);
            return Ok(ApiResponseDto<string>.SuccessResult($"Cache cleared for pattern: {pattern}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache for pattern {Pattern}", pattern);
            return StatusCode(500, ApiResponseDto<string>.ErrorResult($"Failed to clear cache for pattern: {pattern}"));
        }
    }

    // Invalidate dashboard cache
    [HttpPost("invalidate/dashboard")]
    public IActionResult InvalidateDashboardCache()
    {
        try
        {
            _cacheService.InvalidateDashboardCache();
            return Ok(ApiResponseDto<string>.SuccessResult("Dashboard cache invalidated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating dashboard cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to invalidate dashboard cache"));
        }
    }

    // Invalidate student cache
    [HttpPost("invalidate/students")]
    public IActionResult InvalidateStudentCache([FromQuery] int? studentId = null)
    {
        try
        {
            if (studentId.HasValue)
            {
                (_studentService as CachedStudentService)?.InvalidateStudentCache(studentId.Value);
                return Ok(ApiResponseDto<string>.SuccessResult($"Student cache invalidated for ID: {studentId}"));
            }
            else
            {
                _cacheService.InvalidateStudentCache();
                return Ok(ApiResponseDto<string>.SuccessResult("All student caches invalidated successfully"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating student cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to invalidate student cache"));
        }
    }

    // Invalidate teacher cache
    [HttpPost("invalidate/teachers")]
    public IActionResult InvalidateTeacherCache()
    {
        try
        {
            _cacheService.InvalidateTeacherCache();
            return Ok(ApiResponseDto<string>.SuccessResult("Teacher cache invalidated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating teacher cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to invalidate teacher cache"));
        }
    }

    // Invalidate course cache
    [HttpPost("invalidate/courses")]
    public IActionResult InvalidateCourseCache()
    {
        try
        {
            _cacheService.InvalidateCourseCache();
            return Ok(ApiResponseDto<string>.SuccessResult("Course cache invalidated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating course cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to invalidate course cache"));
        }
    }

    // Invalidate grade cache
    [HttpPost("invalidate/grades")]
    public IActionResult InvalidateGradeCache()
    {
        try
        {
            _cacheService.InvalidateGradeCache();
            return Ok(ApiResponseDto<string>.SuccessResult("Grade cache invalidated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating grade cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to invalidate grade cache"));
        }
    }

    // Invalidate enrollment cache
    [HttpPost("invalidate/enrollments")]
    public IActionResult InvalidateEnrollmentCache()
    {
        try
        {
            _cacheService.InvalidateEnrollmentCache();
            return Ok(ApiResponseDto<string>.SuccessResult("Enrollment cache invalidated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating enrollment cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to invalidate enrollment cache"));
        }
    }

    // Warm up cache
    [HttpPost("warmup")]
    public async Task<IActionResult> WarmUpCache()
    {
        try
        {
            await _cacheService.WarmUpCacheAsync();
            return Ok(ApiResponseDto<string>.SuccessResult("Cache warm-up completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to warm up cache"));
        }
    }

    // Warm up dashboard cache
    [HttpPost("warmup/dashboard")]
    public async Task<IActionResult> WarmUpDashboardCache()
    {
        try
        {
            if (_dashboardService is CachedDashboardService cachedDashboard)
            {
                await cachedDashboard.WarmUpCacheAsync();
                return Ok(ApiResponseDto<string>.SuccessResult("Dashboard cache warm-up completed successfully"));
            }
            else
            {
                return BadRequest(ApiResponseDto<string>.ErrorResult("Dashboard service is not cached"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up dashboard cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to warm up dashboard cache"));
        }
    }

    // Warm up student cache
    [HttpPost("warmup/students")]
    public async Task<IActionResult> WarmUpStudentCache()
    {
        try
        {
            if (_studentService is CachedStudentService cachedStudent)
            {
                await cachedStudent.WarmUpStudentCache();
                return Ok(ApiResponseDto<string>.SuccessResult("Student cache warm-up completed successfully"));
            }
            else
            {
                return BadRequest(ApiResponseDto<string>.ErrorResult("Student service is not cached"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up student cache");
            return StatusCode(500, ApiResponseDto<string>.ErrorResult("Failed to warm up student cache"));
        }
    }

    // Get cache performance metrics
    [HttpGet("performance")]
    public async Task<IActionResult> GetCachePerformance()
    {
        try
        {
            var dashboardPerformance = _dashboardService is CachedDashboardService cachedDashboard 
                ? await cachedDashboard.GetCachePerformance() 
                : null;

            var studentPerformance = _studentService is CachedStudentService cachedStudent 
                ? await cachedStudent.GetCachePerformance() 
                : null;

            var performance = new
            {
                Dashboard = dashboardPerformance,
                Students = studentPerformance,
                Overall = new
                {
                    TotalMemoryUsage = _cacheService.GetStatistics().MemoryUsage,
                    OverallHitRate = _cacheService.GetStatistics().HitRate,
                    LastUpdated = DateTime.UtcNow
                }
            };

            return Ok(ApiResponseDto<object>.SuccessResult(performance, "Cache performance metrics retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache performance metrics");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to retrieve cache performance metrics"));
        }
    }

    // Test cache hit/miss
    [HttpGet("test")]
    public async Task<IActionResult> TestCache()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Test dashboard cache
            stopwatch.Restart();
            var dashboard1 = await _dashboardService.GetOverviewStats();
            var firstCall = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Restart();
            var dashboard2 = await _dashboardService.GetOverviewStats();
            var secondCall = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Stop();

            var result = new
            {
                FirstCall = new { ResponseTime = firstCall, FromCache = false },
                SecondCall = new { ResponseTime = secondCall, FromCache = true },
                PerformanceImprovement = firstCall > 0 ? (double)(firstCall - secondCall) / firstCall * 100 : 0,
                CacheWorking = secondCall < firstCall,
                Message = secondCall < firstCall ? "Cache is working correctly" : "Cache may not be working as expected"
            };

            return Ok(ApiResponseDto<object>.SuccessResult(result, "Cache test completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing cache");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to test cache"));
        }
    }

    // Cache documentation
    [HttpGet("docs")]
    [AllowAnonymous]
    public IActionResult GetCacheDocumentation()
    {
        var docs = new
        {
            Title = "Cache API Documentation",
            Version = "1.0.0",
            Description = "Cache management and performance optimization API",
            Endpoints = new[]
            {
                new { Method = "GET", Path = "/api/cache/statistics", Description = "Get cache statistics" },
                new { Method = "POST", Path = "/api/cache/clear", Description = "Clear all cache" },
                new { Method = "POST", Path = "/api/cache/clear/{pattern}", Description = "Clear cache by pattern" },
                new { Method = "POST", Path = "/api/cache/invalidate/dashboard", Description = "Invalidate dashboard cache" },
                new { Method = "POST", Path = "/api/cache/invalidate/students", Description = "Invalidate student cache" },
                new { Method = "POST", Path = "/api/cache/warmup", Description = "Warm up cache" },
                new { Method = "GET", Path = "/api/cache/performance", Description = "Get cache performance metrics" },
                new { Method = "GET", Path = "/api/cache/test", Description = "Test cache functionality" }
            },
            CacheKeys = new
            {
                Dashboard = new[]
                {
                    "dashboard:comprehensive",
                    "dashboard:overview",
                    "dashboard:students",
                    "dashboard:teachers",
                    "dashboard:courses",
                    "dashboard:grades",
                    "dashboard:departments",
                    "dashboard:activity",
                    "dashboard:health"
                },
                Students = new[]
                {
                    "students:list",
                    "students:department:{id}",
                    "students:paginated:{page}:{size}",
                    "students:search:{term}"
                },
                Static = new[]
                {
                    "departments:list",
                    "users:roles",
                    "system:settings"
                }
            },
            ExpirationTimes = new
            {
                Dashboard = "5 minutes",
                QuickStats = "1 minute",
                Lists = "10 minutes",
                StaticData = "1 hour",
                SystemHealth = "2 minutes"
            },
            Features = new
            {
                AutomaticCacheInvalidation = "Cache is automatically invalidated when data changes",
                PerformanceMonitoring = "Track cache hit rates and response times",
                WarmUpCapability = "Pre-populate cache with frequently accessed data",
                PatternBasedInvalidation = "Clear cache entries matching specific patterns",
                MemoryManagement = "Automatic memory management with size limits",
                Logging = "Comprehensive logging for cache operations"
            },
            BestPractices = new[]
            {
                "Use appropriate expiration times based on data volatility",
                "Invalidate cache when data changes",
                "Warm up cache for frequently accessed data",
                "Monitor cache performance regularly",
                "Use shorter cache times for dynamic data",
                "Use longer cache times for static data"
            }
        };

        return Ok(docs);
    }

    // Health check
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "CacheController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "Memory Caching",
                "Cache Invalidation",
                "Performance Monitoring",
                "Cache Warming",
                "Pattern-based Clearing",
                "Statistics Tracking",
                "Health Monitoring"
            }
        });
    }
}
