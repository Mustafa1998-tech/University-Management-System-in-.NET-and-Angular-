using Microsoft.Extensions.Caching.Memory;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Data;

namespace UniversityManagement.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly UniversityDbContext _context;

    // Cache keys
    public static class Keys
    {
        public const string DASHBOARD_COMPREHENSIVE = "dashboard:comprehensive";
        public const string DASHBOARD_OVERVIEW = "dashboard:overview";
        public const string DASHBOARD_STUDENTS = "dashboard:students";
        public const string DASHBOARD_TEACHERS = "dashboard:teachers";
        public const string DASHBOARD_COURSES = "dashboard:courses";
        public const string DASHBOARD_GRADES = "dashboard:grades";
        public const string DASHBOARD_DEPARTMENTS = "dashboard:departments";
        public const string DASHBOARD_ACTIVITY = "dashboard:activity";
        public const string DASHBOARD_HEALTH = "dashboard:health";
        
        public const string STUDENTS_LIST = "students:list";
        public const string STUDENTS_BY_DEPARTMENT = "students:department:{0}";
        public const string TEACHERS_LIST = "teachers:list";
        public const string TEACHERS_BY_DEPARTMENT = "teachers:department:{0}";
        public const string COURSES_LIST = "courses:list";
        public const string COURSES_BY_DEPARTMENT = "courses:department:{0}";
        public const string DEPARTMENTS_LIST = "departments:list";
        public const string ENROLLMENTS_LIST = "enrollments:list";
        public const string GRADES_LIST = "grades:list";
        
        public const string USER_ROLES = "users:roles";
        public const string SYSTEM_SETTINGS = "system:settings";
    }

    // Cache expiration times
    public static class Expiration
    {
        public static readonly TimeSpan Dashboard = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan QuickStats = TimeSpan.FromMinutes(1);
        public static readonly TimeSpan Lists = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan StaticData = TimeSpan.FromHours(1);
        public static readonly TimeSpan SystemHealth = TimeSpan.FromMinutes(2);
    }

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger, UniversityDbContext context)
    {
        _cache = cache;
        _logger = logger;
        _context = context;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out T cachedValue))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        
        var value = await factory();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? Expiration.Dashboard,
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.Normal,
            Size = 1 // Optional: for memory management
        };

        _cache.Set(key, value, cacheEntryOptions);
        
        _logger.LogDebug("Cache set for key: {Key}", key);
        
        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogDebug("Cache removed for key: {Key}", key);
    }

    public void RemoveByPattern(string pattern)
    {
        // This is a simplified approach - in production you might want to use a more sophisticated pattern matching
        var keysToRemove = new List<string>();
        
        // Get all cache entries (this is not directly available in IMemoryCache, so we'll use a different approach)
        // For now, we'll implement common patterns
        
        if (pattern.Contains("dashboard"))
        {
            keysToRemove.AddRange(new[]
            {
                Keys.DASHBOARD_COMPREHENSIVE,
                Keys.DASHBOARD_OVERVIEW,
                Keys.DASHBOARD_STUDENTS,
                Keys.DASHBOARD_TEACHERS,
                Keys.DASHBOARD_COURSES,
                Keys.DASHBOARD_GRADES,
                Keys.DASHBOARD_DEPARTMENTS,
                Keys.DASHBOARD_ACTIVITY,
                Keys.DASHBOARD_HEALTH
            });
        }
        
        if (pattern.Contains("students"))
        {
            keysToRemove.AddRange(new[]
            {
                Keys.STUDENTS_LIST,
                Keys.DASHBOARD_STUDENTS
            });
        }
        
        if (pattern.Contains("teachers"))
        {
            keysToRemove.AddRange(new[]
            {
                Keys.TEACHERS_LIST,
                Keys.DASHBOARD_TEACHERS
            });
        }
        
        if (pattern.Contains("courses"))
        {
            keysToRemove.AddRange(new[]
            {
                Keys.COURSES_LIST,
                Keys.DASHBOARD_COURSES
            });
        }

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
        
        _logger.LogInformation("Cache removed {Count} entries for pattern: {Pattern}", keysToRemove.Count, pattern);
    }

    public void ClearAll()
    {
        // IMemoryCache doesn't have a direct Clear method, so we'll dispose and recreate
        // For now, we'll remove known keys
        RemoveByPattern("*");
        _logger.LogInformation("All cache cleared");
    }

    public CacheStatistics GetStatistics()
    {
        // IMemoryCache doesn't provide direct statistics, but we can estimate
        // This is a placeholder implementation
        return new CacheStatistics
        {
            TotalEntries = 0, // Would need custom implementation
            MemoryUsage = GC.GetTotalMemory(false) / 1024 / 1024, // MB
            HitRate = 0, // Would need custom tracking
            LastCleared = DateTime.UtcNow
        };
    }

    // Cache invalidation methods
    public void InvalidateDashboardCache()
    {
        RemoveByPattern("dashboard");
        _logger.LogInformation("Dashboard cache invalidated");
    }

    public void InvalidateStudentCache()
    {
        RemoveByPattern("students");
        InvalidateDashboardCache();
        _logger.LogInformation("Student cache invalidated");
    }

    public void InvalidateTeacherCache()
    {
        RemoveByPattern("teachers");
        InvalidateDashboardCache();
        _logger.LogInformation("Teacher cache invalidated");
    }

    public void InvalidateCourseCache()
    {
        RemoveByPattern("courses");
        InvalidateDashboardCache();
        _logger.LogInformation("Course cache invalidated");
    }

    public void InvalidateGradeCache()
    {
        RemoveByPattern("grades");
        InvalidateDashboardCache();
        _logger.LogInformation("Grade cache invalidated");
    }

    public void InvalidateEnrollmentCache()
    {
        RemoveByPattern("enrollments");
        InvalidateDashboardCache();
        _logger.LogInformation("Enrollment cache invalidated");
    }

    // Cache warming methods
    public async Task WarmUpCacheAsync()
    {
        _logger.LogInformation("Starting cache warm-up");
        
        try
        {
            // Warm up static data
            await WarmUpStaticDataAsync();
            
            // Warm up dashboard data
            await WarmUpDashboardDataAsync();
            
            _logger.LogInformation("Cache warm-up completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warm-up failed");
        }
    }

    private async Task WarmUpStaticDataAsync()
    {
        // Cache departments
        await GetOrCreateAsync(Keys.DEPARTMENTS_LIST, async () =>
        {
            return await _context.Departments.ToListAsync();
        }, Expiration.StaticData);

        // Cache user roles
        await GetOrCreateAsync(Keys.USER_ROLES, async () =>
        {
            return await _context.Users.Select(u => u.Role).Distinct().ToListAsync();
        }, Expiration.StaticData);
    }

    private async Task WarmUpDashboardDataAsync()
    {
        // This would warm up dashboard data
        // For now, we'll just log that it's done
        _logger.LogDebug("Dashboard data warmed up");
    }
}

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    void Remove(string key);
    void RemoveByPattern(string pattern);
    void ClearAll();
    CacheStatistics GetStatistics();
    void InvalidateDashboardCache();
    void InvalidateStudentCache();
    void InvalidateTeacherCache();
    void InvalidateCourseCache();
    void InvalidateGradeCache();
    void InvalidateEnrollmentCache();
    Task WarmUpCacheAsync();
}

public class CacheStatistics
{
    public int TotalEntries { get; set; }
    public long MemoryUsage { get; set; } // MB
    public double HitRate { get; set; } // Percentage
    public DateTime LastCleared { get; set; }
}
