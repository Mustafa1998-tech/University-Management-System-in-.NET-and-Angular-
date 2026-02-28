using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Infrastructure.Services;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(DashboardService service, ILogger<DashboardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // Comprehensive dashboard with all statistics
    [HttpGet("comprehensive")]
    public async Task<IActionResult> GetComprehensiveDashboard()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var dashboard = await _service.GetComprehensiveDashboard();
            
            stopwatch.Stop();
            
            _logger.LogInformation("Comprehensive dashboard generated in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            
            return Ok(ApiResponseDto<ComprehensiveDashboardDto>.SuccessResult(dashboard, 
                $"Dashboard generated successfully in {stopwatch.ElapsedMilliseconds}ms"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comprehensive dashboard");
            return StatusCode(500, ApiResponseDto<ComprehensiveDashboardDto>.ErrorResult("Failed to generate dashboard"));
        }
    }

    // Overview statistics
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverviewStats()
    {
        try
        {
            var stats = await _service.GetOverviewStats();
            return Ok(ApiResponseDto<DashboardStatsDto>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating overview stats");
            return StatusCode(500, ApiResponseDto<DashboardStatsDto>.ErrorResult("Failed to generate overview stats"));
        }
    }

    // Student statistics
    [HttpGet("students")]
    public async Task<IActionResult> GetStudentStats()
    {
        try
        {
            var stats = await _service.GetStudentStats();
            return Ok(ApiResponseDto<StudentStatsDto>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating student stats");
            return StatusCode(500, ApiResponseDto<StudentStatsDto>.ErrorResult("Failed to generate student stats"));
        }
    }

    // Teacher statistics
    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeacherStats()
    {
        try
        {
            var stats = await _service.GetTeacherStats();
            return Ok(ApiResponseDto<TeacherStatsDto>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating teacher stats");
            return StatusCode(500, ApiResponseDto<TeacherStatsDto>.ErrorResult("Failed to generate teacher stats"));
        }
    }

    // Course statistics
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourseStats()
    {
        try
        {
            var stats = await _service.GetCourseStats();
            return Ok(ApiResponseDto<CourseStatsDto>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating course stats");
            return StatusCode(500, ApiResponseDto<CourseStatsDto>.ErrorResult("Failed to generate course stats"));
        }
    }

    // Grade statistics
    [HttpGet("grades")]
    public async Task<IActionResult> GetGradeStats()
    {
        try
        {
            var stats = await _service.GetGradeStats();
            return Ok(ApiResponseDto<GradeStatsDto>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating grade stats");
            return StatusCode(500, ApiResponseDto<GradeStatsDto>.ErrorResult("Failed to generate grade stats"));
        }
    }

    // Department statistics
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartmentStats()
    {
        try
        {
            var stats = await _service.GetDepartmentStats();
            return Ok(ApiResponseDto<DepartmentStatsDto>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating department stats");
            return StatusCode(500, ApiResponseDto<DepartmentStatsDto>.ErrorResult("Failed to generate department stats"));
        }
    }

    // Activity statistics
    [HttpGet("activity")]
    public async Task<IActionResult> GetActivityStats()
    {
        try
        {
            var stats = await _service.GetActivityStats();
            return Ok(ApiResponseDto<ActivityStatsDto>.SuccessResult(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating activity stats");
            return StatusCode(500, ApiResponseDto<ActivityStatsDto>.ErrorResult("Failed to generate activity stats"));
        }
    }

    // System health check
    [HttpGet("health")]
    public async Task<IActionResult> GetSystemHealth()
    {
        try
        {
            var health = await _service.GetSystemHealth();
            return Ok(ApiResponseDto<SystemHealthDto>.SuccessResult(health));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system health");
            return StatusCode(500, ApiResponseDto<SystemHealthDto>.ErrorResult("Failed to check system health"));
        }
    }

    // Quick stats (for dashboard overview)
    [HttpGet("quick-stats")]
    public async Task<IActionResult> GetQuickStats()
    {
        try
        {
            var stats = await _service.GetOverviewStats();
            
            var quickStats = new
            {
                TotalUsers = stats.TotalUsers,
                TotalStudents = stats.TotalStudents,
                TotalTeachers = stats.TotalTeachers,
                TotalCourses = stats.TotalCourses,
                TotalDepartments = stats.TotalDepartments,
                TotalEnrollments = stats.TotalEnrollments,
                TotalGrades = stats.TotalGrades,
                LastUpdated = stats.LastUpdated,
                DataFreshness = stats.DataFreshness
            };

            return Ok(ApiResponseDto<object>.SuccessResult(quickStats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quick stats");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to generate quick stats"));
        }
    }

    // Department-wise statistics
    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetDepartmentStats(int departmentId)
    {
        try
        {
            // Get specific department statistics
            var allDepartments = await _service.GetDepartmentStats();
            var department = allDepartments.DepartmentOverviews.FirstOrDefault(d => d.DepartmentId == departmentId);

            if (department == null)
                return NotFound(ApiResponseDto<object>.ErrorResult($"Department with ID {departmentId} not found"));

            return Ok(ApiResponseDto<DepartmentOverviewDto>.SuccessResult(department));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating department stats for department {DepartmentId}", departmentId);
            return StatusCode(500, ApiResponseDto<DepartmentOverviewDto>.ErrorResult($"Failed to generate stats for department {departmentId}"));
        }
    }

    // Teacher workload statistics
    [HttpGet("teacher-workload")]
    public async Task<IActionResult> GetTeacherWorkload()
    {
        try
        {
            var teacherStats = await _service.GetTeacherStats();
            return Ok(ApiResponseDto<List<TeacherWorkloadDto>>.SuccessResult(teacherStats.TeacherWorkloads));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating teacher workload stats");
            return StatusCode(500, ApiResponseDto<List<TeacherWorkloadDto>>.ErrorResult("Failed to generate teacher workload stats"));
        }
    }

    // Course enrollment statistics
    [HttpGet("course-enrollments")]
    public async Task<IActionResult> GetCourseEnrollments()
    {
        try
        {
            var courseStats = await _service.GetCourseStats();
            
            var enrollmentStats = new
            {
                TopEnrolled = courseStats.TopEnrolledCourses,
                LeastEnrolled = courseStats.LeastEnrolledCourses,
                AverageStudentsPerCourse = courseStats.AverageStudentsPerCourse,
                CoursesWithoutTeacher = courseStats.CoursesWithoutTeacher,
                TotalCourses = courseStats.TotalCourses
            };

            return Ok(ApiResponseDto<object>.SuccessResult(enrollmentStats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating course enrollment stats");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to generate course enrollment stats"));
        }
    }

    // Grade distribution statistics
    [HttpGet("grade-distribution")]
    public async Task<IActionResult> GetGradeDistribution()
    {
        try
        {
            var gradeStats = await _service.GetGradeStats();
            
            var distribution = new
            {
                TotalGrades = gradeStats.TotalGrades,
                AverageGrade = gradeStats.AverageGrade,
                GradeDistribution = gradeStats.GradeDistribution,
                SubjectPerformance = gradeStats.SubjectPerformance.Take(10), // Top 10 performing subjects
                HighestGrade = gradeStats.HighestGrade,
                LowestGrade = gradeStats.LowestGrade
            };

            return Ok(ApiResponseDto<object>.SuccessResult(distribution));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating grade distribution stats");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to generate grade distribution stats"));
        }
    }

    // Recent activity
    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity()
    {
        try
        {
            var activityStats = await _service.GetActivityStats();
            
            var recentActivity = new
            {
                Last24Hours = new
                {
                    Registrations = activityStats.RegistrationsLast24Hours,
                    GradesSubmitted = activityStats.GradesSubmittedLast24Hours,
                    Enrollments = activityStats.EnrollmentsLast24Hours
                },
                Last7Days = new
                {
                    Registrations = activityStats.RegistrationsLast7Days,
                    GradesSubmitted = activityStats.GradesSubmittedLast7Days,
                    Enrollments = activityStats.EnrollmentsLast7Days
                },
                Last30Days = new
                {
                    Registrations = activityStats.RegistrationsLast30Days,
                    GradesSubmitted = activityStats.GradesSubmittedLast30Days,
                    Enrollments = activityStats.EnrollmentsLast30Days
                },
                DailyActivity = activityStats.DailyActivity.Take(7) // Last 7 days
            };

            return Ok(ApiResponseDto<object>.SuccessResult(recentActivity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recent activity stats");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to generate recent activity stats"));
        }
    }

    // Performance metrics
    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformanceMetrics()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var dashboard = await _service.GetComprehensiveDashboard();
            
            stopwatch.Stop();

            var performanceMetrics = new
            {
                DashboardGenerationTime = dashboard.GenerationTime,
                TotalRecords = dashboard.Overview.TotalUsers + dashboard.Overview.TotalStudents + dashboard.Overview.TotalTeachers,
                RecordsPerSecond = (dashboard.Overview.TotalUsers + dashboard.Overview.TotalStudents + dashboard.Overview.TotalTeachers) / stopwatch.Elapsed.TotalSeconds,
                DatabaseQueries = 8, // Approximate number of queries executed
                MemoryUsage = GC.GetTotalMemory(false) / 1024 / 1024, // MB
                CpuTime = stopwatch.Elapsed.TotalMilliseconds,
                GeneratedAt = dashboard.GeneratedAt
            };

            return Ok(ApiResponseDto<object>.SuccessResult(performanceMetrics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance metrics");
            return StatusCode(500, ApiResponseDto<object>.ErrorResult("Failed to generate performance metrics"));
        }
    }

    // Dashboard documentation
    [HttpGet("docs")]
    [AllowAnonymous]
    public IActionResult GetDashboardDocumentation()
    {
        var docs = new
        {
            Title = "Dashboard API Documentation",
            Version = "1.0.0",
            Description = "Comprehensive dashboard statistics for University Management System",
            Endpoints = new[]
            {
                new { Method = "GET", Path = "/api/dashboard/comprehensive", Description = "Complete dashboard with all statistics" },
                new { Method = "GET", Path = "/api/dashboard/overview", Description = "Overview statistics" },
                new { Method = "GET", Path = "/api/dashboard/students", Description = "Student statistics" },
                new { Method = "GET", Path = "/api/dashboard/teachers", Description = "Teacher statistics" },
                new { Method = "GET", Path = "/api/dashboard/courses", Description = "Course statistics" },
                new { Method = "GET", Path = "/api/dashboard/grades", Description = "Grade statistics" },
                new { Method = "GET", Path = "/api/dashboard/departments", Description = "Department statistics" },
                new { Method = "GET", Path = "/api/dashboard/activity", Description = "Activity statistics" },
                new { Method = "GET", Path = "/api/dashboard/health", Description = "System health check" },
                new { Method = "GET", Path = "/api/dashboard/quick-stats", Description = "Quick overview stats" }
            },
            Features = new
            {
                RealTimeData = "Statistics are calculated in real-time from the database",
                PerformanceOptimized = "Efficient queries with proper indexing",
                ComprehensiveCoverage = "All major entities and activities tracked",
                ErrorHandling = "Robust error handling with detailed logging",
                ResponseTime = "Optimized for dashboard loading speed",
                CachingReady = "Structure supports easy caching implementation"
            },
            Examples = new[]
            {
                new
                {
                    Description = "Get complete dashboard",
                    Url = "/api/dashboard/comprehensive",
                    ResponseTime = "< 500ms"
                },
                new
                {
                    Description = "Get quick overview",
                    Url = "/api/dashboard/quick-stats",
                    ResponseTime = "< 100ms"
                },
                new
                {
                    Description = "Get system health",
                    Url = "/api/dashboard/health",
                    ResponseTime = "< 50ms"
                }
            },
            Authentication = new
            {
                Required = true,
                Roles = new[] { "Admin" },
                Description = "Only administrators can access dashboard statistics"
            },
            RateLimiting = new
            {
                Enabled = false,
                Description = "No rate limiting implemented (can be added if needed)"
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
            Service = "DashboardController",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Features = new[]
            {
                "Real-time Statistics",
                "Comprehensive Analytics",
                "Performance Monitoring",
                "System Health Checks",
                "Activity Tracking",
                "Error Handling",
                "Response Time Optimization"
            }
        });
    }
}
