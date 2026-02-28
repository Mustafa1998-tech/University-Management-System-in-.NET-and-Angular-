using Microsoft.EntityFrameworkCore;
using UniversityManagement.Application.DTOs;
using UniversityManagement.Domain.Entities;
using UniversityManagement.Infrastructure.Data;
using System.Diagnostics;

namespace UniversityManagement.Infrastructure.Services;

public class DashboardService
{
    private readonly UniversityDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(UniversityDbContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ComprehensiveDashboardDto> GetComprehensiveDashboard()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var overviewTask = GetOverviewStats();
            var studentTask = GetStudentStats();
            var teacherTask = GetTeacherStats();
            var courseTask = GetCourseStats();
            var gradeTask = GetGradeStats();
            var departmentTask = GetDepartmentStats();
            var activityTask = GetActivityStats();
            var healthTask = GetSystemHealth();

            await Task.WhenAll(
                overviewTask,
                studentTask,
                teacherTask,
                courseTask,
                gradeTask,
                departmentTask,
                activityTask,
                healthTask
            );

            stopwatch.Stop();

            return new ComprehensiveDashboardDto
            {
                Overview = overviewTask.Result,
                Students = studentTask.Result,
                Teachers = teacherTask.Result,
                Courses = courseTask.Result,
                Grades = gradeTask.Result,
                Departments = departmentTask.Result,
                Activity = activityTask.Result,
                Health = healthTask.Result,
                GeneratedAt = DateTime.UtcNow,
                GenerationTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comprehensive dashboard");
            throw;
        }
    }

    public async Task<DashboardStatsDto> GetOverviewStats()
    {
        var now = DateTime.UtcNow;
        
        var totalStudents = await _context.Students.CountAsync();
        var totalTeachers = await _context.Teachers.CountAsync();
        var totalCourses = await _context.Courses.CountAsync();
        var totalDepartments = await _context.Departments.CountAsync();
        var totalEnrollments = await _context.StudentCourses.CountAsync();
        var totalGrades = await _context.Grades.CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        return new DashboardStatsDto
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalCourses = totalCourses,
            TotalDepartments = totalDepartments,
            TotalEnrollments = totalEnrollments,
            TotalGrades = totalGrades,
            TotalUsers = totalUsers,
            LastUpdated = now,
            DataFreshness = TimeSpan.FromMinutes(5) // Assuming data is cached for 5 minutes
        };
    }

    public async Task<StudentStatsDto> GetStudentStats()
    {
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);

        var totalStudents = await _context.Students.CountAsync();
        
        // Students with grades (active students)
        var studentsWithGrades = await _context.Grades
            .Select(g => g.StudentId)
            .Distinct()
            .CountAsync();

        // New students this month
        var newStudentsThisMonth = await _context.Students
            .Join(_context.Users, s => s.UserId, u => u.Id, (s, u) => new { s, u })
            .CountAsync(x => x.u.CreatedAt >= lastMonth);

        // Average grade
        var grades = await _context.Grades.ToListAsync();
        var averageGrade = grades.Any() ? grades.Average(g => g.Value) : 0;

        // Department distribution
        var departmentDistribution = await _context.Students
            .Join(_context.Departments, s => s.DepartmentId, d => d.Id, (s, d) => new { s, d })
            .GroupBy(x => x.d.Id)
            .Select(g => new DepartmentStudentCountDto
            {
                DepartmentId = g.Key,
                DepartmentName = g.First().d.Name,
                StudentCount = g.Count(),
                TeacherCount = _context.Teachers.Count(t => t.DepartmentId == g.Key),
                CourseCount = _context.Courses.Count(c => c.DepartmentId == g.Key),
                AverageGrade = _context.Grades
                    .Join(_context.Students, gr => gr.StudentId, s => s.Id, (gr, s) => new { gr, s })
                    .Join(_context.Courses, x => x.s.Id, c => c.Id, (x, c) => new { x.gr, c })
                    .Join(_context.Departments, x => x.c.DepartmentId, d => d.Id, (x, d) => new { x, d })
                    .Where(x => x.d.Id == g.Key)
                    .Average(x => x.x.gr.Value)
            })
            .OrderByDescending(d => d.StudentCount)
            .ToListAsync();

        return new StudentStatsDto
        {
            TotalStudents = totalStudents,
            ActiveStudents = studentsWithGrades,
            NewStudentsThisMonth = newStudentsThisMonth,
            StudentsByDepartment = departmentDistribution.Sum(d => d.StudentCount),
            AverageGrade = averageGrade,
            StudentsWithGrades = studentsWithGrades,
            DepartmentDistribution = departmentDistribution
        };
    }

    public async Task<TeacherStatsDto> GetTeacherStats()
    {
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);

        var totalTeachers = await _context.Teachers.CountAsync();

        // New teachers this month
        var newTeachersThisMonth = await _context.Teachers
            .Join(_context.Users, t => t.UserId, u => u.Id, (t, u) => new { t, u })
            .CountAsync(x => x.u.CreatedAt >= lastMonth);

        // Average students per teacher
        var totalStudents = await _context.Students.CountAsync();
        var averageStudentsPerTeacher = totalTeachers > 0 ? (double)totalStudents / totalTeachers : 0;

        // Teacher workloads
        var teacherWorkloads = await _context.Teachers
            .Include(t => t.Department)
            .Select(t => new TeacherWorkloadDto
            {
                TeacherId = t.Id,
                TeacherName = t.FullName,
                DepartmentName = t.Department.Name,
                CoursesCount = _context.Courses.Count(c => c.TeacherId == t.Id),
                TotalStudents = _context.StudentCourses
                    .Join(_context.Courses, sc => sc.CourseId, c => c.Id, (sc, c) => new { sc, c })
                    .Where(x => x.c.TeacherId == t.Id)
                    .Select(x => x.sc.StudentId)
                    .Distinct()
                    .Count(),
                AverageStudentsPerCourse = 0, // Calculated below
                TotalGrades = _context.Grades
                    .Join(_context.Students, g => g.StudentId, s => s.Id, (g, s) => new { g, s })
                    .Join(_context.Courses, x => x.s.Id, c => c.Id, (x, c) => new { x, c })
                    .Where(x => x.c.TeacherId == t.Id)
                    .Count()
            })
            .ToListAsync();

        // Calculate average students per course for each teacher
        foreach (var workload in teacherWorkloads)
        {
            workload.AverageStudentsPerCourse = workload.CoursesCount > 0 
                ? (double)workload.TotalStudents / workload.CoursesCount 
                : 0;
        }

        return new TeacherStatsDto
        {
            TotalTeachers = totalTeachers,
            ActiveTeachers = teacherWorkloads.Count(w => w.CoursesCount > 0),
            NewTeachersThisMonth = newTeachersThisMonth,
            TeachersByDepartment = teacherWorkloads.GroupBy(t => t.DepartmentName).Count(),
            AverageStudentsPerTeacher = averageStudentsPerTeacher,
            TeacherWorkloads = teacherWorkloads.OrderByDescending(w => w.TotalStudents).ToList()
        };
    }

    public async Task<CourseStatsDto> GetCourseStats()
    {
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);

        var totalCourses = await _context.Courses.CountAsync();

        // New courses this month
        var newCoursesThisMonth = await _context.Courses
            .CountAsync(c => c.CreatedAt >= lastMonth);

        // Average students per course
        var totalEnrollments = await _context.StudentCourses.CountAsync();
        var averageStudentsPerCourse = totalCourses > 0 ? (double)totalEnrollments / totalCourses : 0;

        // Courses without teacher
        var coursesWithoutTeacher = await _context.Courses
            .CountAsync(c => c.TeacherId == null);

        // Top enrolled courses
        var topEnrolledCourses = await _context.StudentCourses
            .Join(_context.Courses, sc => sc.CourseId, c => c.Id, (sc, c) => new { sc, c })
            .Join(_context.Departments, x => x.c.DepartmentId, d => d.Id, (x, d) => new { x.sc, x.c, d })
            .Join(_context.Teachers, x => x.c.TeacherId, t => t.Id, (x, t) => new { x.sc, x.c, x.d, t })
            .GroupBy(x => x.c.Id)
            .Select(g => new CourseEnrollmentDto
            {
                CourseId = g.Key,
                CourseName = g.First().c.Name,
                DepartmentName = g.First().d.Name,
                TeacherName = g.First().t.FullName,
                EnrollmentCount = g.Count(),
                AverageGrade = _context.Grades
                    .Where(gr => gr.CourseId == g.Key)
                    .DefaultIfEmpty()
                    .Average(gr => gr.Value),
                MaxCapacity = 50, // This could be a property of Course entity
                CapacityUtilization = g.Count() * 2.0 // Assuming max capacity of 50
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(10)
            .ToListAsync();

        // Least enrolled courses
        var leastEnrolledCourses = await _context.StudentCourses
            .Join(_context.Courses, sc => sc.CourseId, c => c.Id, (sc, c) => new { sc, c })
            .Join(_context.Departments, x => x.c.DepartmentId, d => d.Id, (x, d) => new { x.sc, x.c, d })
            .Join(_context.Teachers, x => x.c.TeacherId, t => t.Id, (x, t) => new { x.sc, x.c, x.d, t })
            .GroupBy(x => x.c.Id)
            .Select(g => new CourseEnrollmentDto
            {
                CourseId = g.Key,
                CourseName = g.First().c.Name,
                DepartmentName = g.First().d.Name,
                TeacherName = g.First().t.FullName,
                EnrollmentCount = g.Count(),
                AverageGrade = _context.Grades
                    .Where(gr => gr.CourseId == g.Key)
                    .DefaultIfEmpty()
                    .Average(gr => gr.Value),
                MaxCapacity = 50,
                CapacityUtilization = g.Count() * 2.0
            })
            .OrderBy(c => c.EnrollmentCount)
            .Take(10)
            .ToListAsync();

        return new CourseStatsDto
        {
            TotalCourses = totalCourses,
            ActiveCourses = topEnrolledCourses.Count + leastEnrolledCourses.Count,
            NewCoursesThisMonth = newCoursesThisMonth,
            AverageStudentsPerCourse = averageStudentsPerCourse,
            CoursesWithoutTeacher = coursesWithoutTeacher,
            TopEnrolledCourses = topEnrolledCourses,
            LeastEnrolledCourses = leastEnrolledCourses
        };
    }

    public async Task<GradeStatsDto> GetGradeStats()
    {
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);

        var grades = await _context.Grades.ToListAsync();
        
        var totalGrades = grades.Count;
        var averageGrade = grades.Any() ? grades.Average(g => g.Value) : 0;
        var highestGrade = grades.Any() ? grades.Max(g => g.Value) : 0;
        var lowestGrade = grades.Any() ? grades.Min(g => g.Value) : 0;

        // Grades this month
        var gradesThisMonth = grades.Count(g => g.GradeDate >= lastMonth);

        // Grade distribution
        var gradeDistribution = new Dictionary<string, int>
        {
            { "A+", grades.Count(g => g.Value >= 90) },
            { "A", grades.Count(g => g.Value >= 85 && g.Value < 90) },
            { "B+", grades.Count(g => g.Value >= 80 && g.Value < 85) },
            { "B", grades.Count(g => g.Value >= 75 && g.Value < 80) },
            { "B-", grades.Count(g => g.Value >= 70 && g.Value < 75) },
            { "C+", grades.Count(g => g.Value >= 65 && g.Value < 70) },
            { "C", grades.Count(g => g.Value >= 60 && g.Value < 65) },
            { "C-", grades.Count(g => g.Value >= 55 && g.Value < 60) },
            { "D+", grades.Count(g => g.Value >= 45 && g.Value < 55) },
            { "D", grades.Count(g => g.Value >= 40 && g.Value < 45) },
            { "F", grades.Count(g => g.Value < 40) }
        };

        // Subject performance
        var subjectPerformance = await _context.Grades
            .Join(_context.Courses, g => g.CourseId, c => c.Id, (g, c) => new { g, c })
            .GroupBy(x => x.c.Id)
            .Select(g => new SubjectPerformanceDto
            {
                CourseName = g.First().c.Name,
                StudentCount = g.Select(x => x.g.StudentId).Distinct().Count(),
                AverageGrade = g.Average(x => x.g.Value),
                HighestGrade = g.Max(x => x.g.Value),
                LowestGrade = g.Min(x => x.g.Value),
                GradeCount = g.Count()
            })
            .OrderByDescending(s => s.AverageGrade)
            .ToListAsync();

        return new GradeStatsDto
        {
            TotalGrades = totalGrades,
            AverageGrade = averageGrade,
            HighestGrade = highestGrade,
            LowestGrade = lowestGrade,
            GradesThisMonth = gradesThisMonth,
            GradeDistribution = gradeDistribution,
            SubjectPerformance = subjectPerformance
        };
    }

    public async Task<DepartmentStatsDto> GetDepartmentStats()
    {
        var departments = await _context.Departments.ToListAsync();
        
        var departmentOverviews = await _context.Departments
            .Select(d => new DepartmentOverviewDto
            {
                DepartmentId = d.Id,
                DepartmentName = d.Name,
                StudentCount = _context.Students.Count(s => s.DepartmentId == d.Id),
                TeacherCount = _context.Teachers.Count(t => t.DepartmentId == d.Id),
                CourseCount = _context.Courses.Count(c => c.DepartmentId == d.Id),
                EnrollmentCount = _context.StudentCourses
                    .Join(_context.Courses, sc => sc.CourseId, c => c.Id, (sc, c) => new { sc, c })
                    .Where(x => x.c.DepartmentId == d.Id)
                    .Count(),
                AverageGrade = _context.Grades
                    .Join(_context.Students, g => g.StudentId, s => s.Id, (g, s) => new { g, s })
                    .Join(_context.Courses, x => x.s.Id, c => c.Id, (x, c) => new { x, c })
                    .Where(x => x.c.DepartmentId == d.Id)
                    .DefaultIfEmpty()
                    .Average(x => x.x.g.Value)
            })
            .ToListAsync();

        // Calculate performance score
        foreach (var overview in departmentOverviews)
        {
            overview.PerformanceScore = CalculatePerformanceScore(overview);
        }

        return new DepartmentStatsDto
        {
            TotalDepartments = departments.Count,
            ActiveDepartments = departmentOverviews.Count(d => d.StudentCount > 0),
            AverageStudentsPerDepartment = departments.Any() ? (double)departmentOverviews.Sum(d => d.StudentCount) / departments.Count : 0,
            AverageTeachersPerDepartment = departments.Any() ? (double)departmentOverviews.Sum(d => d.TeacherCount) / departments.Count : 0,
            AverageCoursesPerDepartment = departments.Any() ? (double)departmentOverviews.Sum(d => d.CourseCount) / departments.Count : 0,
            DepartmentOverviews = departmentOverviews.OrderByDescending(d => d.PerformanceScore).ToList()
        };
    }

    public async Task<ActivityStatsDto> GetActivityStats()
    {
        var now = DateTime.UtcNow;
        var last24Hours = now.AddHours(-24);
        var last7Days = now.AddDays(-7);
        var last30Days = now.AddDays(-30);

        // Logins (this would require tracking login activity in a separate table)
        var loginsLast24Hours = 0; // Placeholder - would query login activity table
        var loginsLast7Days = 0;
        var loginsLast30Days = 0;

        // Registrations
        var registrationsLast24Hours = await _context.Users.CountAsync(u => u.CreatedAt >= last24Hours);
        var registrationsLast7Days = await _context.Users.CountAsync(u => u.CreatedAt >= last7Days);
        var registrationsLast30Days = await _context.Users.CountAsync(u => u.CreatedAt >= last30Days);

        // Grades submitted
        var gradesSubmittedLast24Hours = await _context.Grades.CountAsync(g => g.GradeDate >= last24Hours);
        var gradesSubmittedLast7Days = await _context.Grades.CountAsync(g => g.GradeDate >= last7Days);
        var gradesSubmittedLast30Days = await _context.Grades.CountAsync(g => g.GradeDate >= last30Days);

        // Enrollments
        var enrollmentsLast24Hours = await _context.StudentCourses.CountAsync(sc => sc.EnrollmentDate >= last24Hours);
        var enrollmentsLast7Days = await _context.StudentCourses.CountAsync(sc => sc.EnrollmentDate >= last7Days);
        var enrollmentsLast30Days = await _context.StudentCourses.CountAsync(sc => sc.EnrollmentDate >= last30Days);

        // Daily activity for the last 30 days
        var dailyActivity = new List<DailyActivityDto>();
        for (int i = 29; i >= 0; i--)
        {
            var date = now.Date.AddDays(-i);
            var dayStart = date;
            var dayEnd = date.AddDays(1);

            dailyActivity.Add(new DailyActivityDto
            {
                Date = date,
                Logins = 0, // Placeholder
                Registrations = await _context.Users.CountAsync(u => u.CreatedAt >= dayStart && u.CreatedAt < dayEnd),
                GradesSubmitted = await _context.Grades.CountAsync(g => g.GradeDate >= dayStart && g.GradeDate < dayEnd),
                Enrollments = await _context.StudentCourses.CountAsync(sc => sc.EnrollmentDate >= dayStart && sc.EnrollmentDate < dayEnd)
            });
        }

        return new ActivityStatsDto
        {
            LoginsLast24Hours = loginsLast24Hours,
            LoginsLast7Days = loginsLast7Days,
            LoginsLast30Days = loginsLast30Days,
            RegistrationsLast24Hours = registrationsLast24Hours,
            RegistrationsLast7Days = registrationsLast7Days,
            RegistrationsLast30Days = registrationsLast30Days,
            GradesSubmittedLast24Hours = gradesSubmittedLast24Hours,
            GradesSubmittedLast7Days = gradesSubmittedLast7Days,
            GradesSubmittedLast30Days = gradesSubmittedLast30Days,
            DailyActivity = dailyActivity
        };
    }

    public async Task<SystemHealthDto> GetSystemHealth()
    {
        var warnings = new List<string>();
        var databaseHealthy = true;
        
        try
        {
            // Test database connectivity
            await _context.Database.CanConnectAsync();
            databaseHealthy = true;
        }
        catch (Exception ex)
        {
            databaseHealthy = false;
            warnings.Add($"Database connection failed: {ex.Message}");
        }

        // Check database size (approximate)
        long databaseSize = 0;
        try
        {
            databaseSize = await GetDatabaseSize();
            if (databaseSize > 1024 * 1024 * 1024) // 1GB
            {
                warnings.Add("Database size exceeds 1GB");
            }
        }
        catch (Exception ex)
        {
            warnings.Add($"Could not check database size: {ex.Message}");
        }

        // Check active connections
        var activeConnections = 0; // Placeholder - would need connection tracking

        return new SystemHealthDto
        {
            DatabaseHealthy = databaseHealthy,
            AuthenticationHealthy = true, // Placeholder
            FileStorageHealthy = true, // Placeholder
            DatabaseSize = databaseSize,
            ActiveConnections = activeConnections,
            AverageResponseTime = TimeSpan.FromMilliseconds(150), // Placeholder
            LastHealthCheck = DateTime.UtcNow,
            Warnings = warnings
        };
    }

    private double CalculatePerformanceScore(DepartmentOverviewDto department)
    {
        // Simple performance score calculation based on multiple factors
        var gradeScore = department.AverageGrade > 0 ? department.AverageGrade / 100 * 40 : 0;
        var enrollmentScore = department.EnrollmentCount > 0 ? Math.Min(department.EnrollmentCount / 50.0, 30) : 0;
        var courseScore = department.CourseCount > 0 ? Math.Min(department.CourseCount / 10.0, 20) : 0;
        var teacherScore = department.TeacherCount > 0 ? Math.Min(department.TeacherCount / 5.0, 10) : 0;

        return gradeScore + enrollmentScore + courseScore + teacherScore;
    }

    private async Task<long> GetDatabaseSize()
    {
        // This is a simplified approach - in production you'd use database-specific queries
        try
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT SUM(size) * 8192 FROM sys.master_files WHERE type = 0";
            var size = (long)await command.ExecuteScalarAsync();
            
            await connection.CloseAsync();
            return size;
        }
        catch
        {
            return 0;
        }
    }
}
