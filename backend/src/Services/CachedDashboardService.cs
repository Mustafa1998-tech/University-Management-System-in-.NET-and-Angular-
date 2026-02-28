using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;
using System.Diagnostics;

namespace UniversityManagement.Infrastructure.Services;

public class CachedDashboardService : IDashboardService
{
    private readonly UniversityDbContext _context;
    private readonly ILogger<CachedDashboardService> _logger;
    private readonly ICacheService _cacheService;
    private readonly DashboardService _baseDashboardService;

    public CachedDashboardService(
        UniversityDbContext context, 
        ILogger<CachedDashboardService> logger,
        ICacheService cacheService,
        DashboardService baseDashboardService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
        _baseDashboardService = baseDashboardService;
    }

    public async Task<ComprehensiveDashboardDto> GetComprehensiveDashboard()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_COMPREHENSIVE,
            async () =>
            {
                _logger.LogDebug("Generating comprehensive dashboard from database");
                return await _baseDashboardService.GetComprehensiveDashboard();
            },
            CacheService.Expiration.Dashboard
        );
    }

    public async Task<DashboardStatsDto> GetOverviewStats()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_OVERVIEW,
            async () =>
            {
                _logger.LogDebug("Generating overview stats from database");
                return await _baseDashboardService.GetOverviewStats();
            },
            CacheService.Expiration.QuickStats
        );
    }

    public async Task<StudentStatsDto> GetStudentStats()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_STUDENTS,
            async () =>
            {
                _logger.LogDebug("Generating student stats from database");
                return await _baseDashboardService.GetStudentStats();
            },
            CacheService.Expiration.Dashboard
        );
    }

    public async Task<TeacherStatsDto> GetTeacherStats()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_TEACHERS,
            async () =>
            {
                _logger.LogDebug("Generating teacher stats from database");
                return await _baseDashboardService.GetTeacherStats();
            },
            CacheService.Expiration.Dashboard
        );
    }

    public async Task<CourseStatsDto> GetCourseStats()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_COURSES,
            async () =>
            {
                _logger.LogDebug("Generating course stats from database");
                return await _baseDashboardService.GetCourseStats();
            },
            CacheService.Expiration.Dashboard
        );
    }

    public async Task<GradeStatsDto> GetGradeStats()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_GRADES,
            async () =>
            {
                _logger.LogDebug("Generating grade stats from database");
                return await _baseDashboardService.GetGradeStats();
            },
            CacheService.Expiration.Dashboard
        );
    }

    public async Task<DepartmentStatsDto> GetDepartmentStats()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_DEPARTMENTS,
            async () =>
            {
                _logger.LogDebug("Generating department stats from database");
                return await _baseDashboardService.GetDepartmentStats();
            },
            CacheService.Expiration.Dashboard
        );
    }

    public async Task<ActivityStatsDto> GetActivityStats()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_ACTIVITY,
            async () =>
            {
                _logger.LogDebug("Generating activity stats from database");
                return await _baseDashboardService.GetActivityStats();
            },
            CacheService.Expiration.Dashboard
        );
    }

    public async Task<SystemHealthDto> GetSystemHealth()
    {
        return await _cacheService.GetOrCreateAsync(
            CacheService.Keys.DASHBOARD_HEALTH,
            async () =>
            {
                _logger.LogDebug("Checking system health from database");
                return await _baseDashboardService.GetSystemHealth();
            },
            CacheService.Expiration.SystemHealth
        );
    }

    // Cache management methods
    public void InvalidateAllCaches()
    {
        _cacheService.InvalidateDashboardCache();
        _logger.LogInformation("All dashboard caches invalidated");
    }

    public void InvalidateStudentRelatedCaches()
    {
        _cacheService.InvalidateStudentCache();
        _logger.LogInformation("Student-related caches invalidated");
    }

    public void InvalidateTeacherRelatedCaches()
    {
        _cacheService.InvalidateTeacherCache();
        _logger.LogInformation("Teacher-related caches invalidated");
    }

    public void InvalidateCourseRelatedCaches()
    {
        _cacheService.InvalidateCourseCache();
        _logger.LogInformation("Course-related caches invalidated");
    }

    public void InvalidateGradeRelatedCaches()
    {
        _cacheService.InvalidateGradeCache();
        _logger.LogInformation("Grade-related caches invalidated");
    }

    public void InvalidateEnrollmentRelatedCaches()
    {
        _cacheService.InvalidateEnrollmentCache();
        _logger.LogInformation("Enrollment-related caches invalidated");
    }

    // Cache statistics
    public CacheStatistics GetCacheStatistics()
    {
        return _cacheService.GetStatistics();
    }

    // Cache warming
    public async Task WarmUpCacheAsync()
    {
        _logger.LogInformation("Starting dashboard cache warm-up");
        
        try
        {
            // Warm up frequently accessed data
            var tasks = new[]
            {
                GetOverviewStats(),
                GetStudentStats(),
                GetTeacherStats(),
                GetCourseStats(),
                GetGradeStats(),
                GetDepartmentStats()
            };

            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Dashboard cache warm-up completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard cache warm-up failed");
        }
    }

    // Performance monitoring
    public async Task<CachePerformanceDto> GetCachePerformance()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Test cache performance
        var cachedResult = await GetOverviewStats();
        var cacheTime = stopwatch.ElapsedMilliseconds;
        
        stopwatch.Restart();
        
        // Invalidate cache and test database performance
        _cacheService.Remove(CacheService.Keys.DASHBOARD_OVERVIEW);
        var dbResult = await GetOverviewStats();
        var dbTime = stopwatch.ElapsedMilliseconds;
        
        stopwatch.Stop();

        return new CachePerformanceDto
        {
            CacheResponseTime = cacheTime,
            DatabaseResponseTime = dbTime,
            PerformanceImprovement = dbTime > 0 ? (double)(dbTime - cacheTime) / dbTime * 100 : 0,
            CacheHitRate = GetCacheStatistics().HitRate,
            MemoryUsage = GetCacheStatistics().MemoryUsage,
            LastUpdated = DateTime.UtcNow
        };
    }
}

public interface IDashboardService
{
    Task<ComprehensiveDashboardDto> GetComprehensiveDashboard();
    Task<DashboardStatsDto> GetOverviewStats();
    Task<StudentStatsDto> GetStudentStats();
    Task<TeacherStatsDto> GetTeacherStats();
    Task<CourseStatsDto> GetCourseStats();
    Task<GradeStatsDto> GetGradeStats();
    Task<DepartmentStatsDto> GetDepartmentStats();
    Task<ActivityStatsDto> GetActivityStats();
    Task<SystemHealthDto> GetSystemHealth();
}

public class CachePerformanceDto
{
    public long CacheResponseTime { get; set; } // ms
    public long DatabaseResponseTime { get; set; } // ms
    public double PerformanceImprovement { get; set; } // percentage
    public double CacheHitRate { get; set; } // percentage
    public long MemoryUsage { get; set; } // MB
    public DateTime LastUpdated { get; set; }
}
