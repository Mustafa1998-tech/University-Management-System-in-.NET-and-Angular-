namespace UniversityManagement.Application.DTOs;

// Main dashboard statistics
public class DashboardStatsDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalCourses { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalGrades { get; set; }
    public int TotalUsers { get; set; }
    public DateTime LastUpdated { get; set; }
    public TimeSpan DataFreshness { get; set; }
}

// Student statistics
public class StudentStatsDto
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int NewStudentsThisMonth { get; set; }
    public int StudentsByDepartment { get; set; }
    public double AverageGrade { get; set; }
    public int StudentsWithGrades { get; set; }
    public List<DepartmentStudentCountDto> DepartmentDistribution { get; set; }
}

// Teacher statistics
public class TeacherStatsDto
{
    public int TotalTeachers { get; set; }
    public int ActiveTeachers { get; set; }
    public int NewTeachersThisMonth { get; set; }
    public int TeachersByDepartment { get; set; }
    public double AverageStudentsPerTeacher { get; set; }
    public List<TeacherWorkloadDto> TeacherWorkloads { get; set; }
}

// Course statistics
public class CourseStatsDto
{
    public int TotalCourses { get; set; }
    public int ActiveCourses { get; set; }
    public int NewCoursesThisMonth { get; set; }
    public double AverageStudentsPerCourse { get; set; }
    public int CoursesWithoutTeacher { get; set; }
    public List<CourseEnrollmentDto> TopEnrolledCourses { get; set; }
    public List<CourseEnrollmentDto> LeastEnrolledCourses { get; set; }
}

// Grade statistics
public class GradeStatsDto
{
    public int TotalGrades { get; set; }
    public double AverageGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
    public int GradesThisMonth { get; set; }
    public Dictionary<string, int> GradeDistribution { get; set; }
    public List<SubjectPerformanceDto> SubjectPerformance { get; set; }
}

// Department statistics
public class DepartmentStatsDto
{
    public int TotalDepartments { get; set; }
    public int ActiveDepartments { get; set; }
    public double AverageStudentsPerDepartment { get; set; }
    public double AverageTeachersPerDepartment { get; set; }
    public double AverageCoursesPerDepartment { get; set; }
    public List<DepartmentOverviewDto> DepartmentOverviews { get; set; }
}

// Supporting DTOs
public class DepartmentStudentCountDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int CourseCount { get; set; }
    public double AverageGrade { get; set; }
}

public class TeacherWorkloadDto
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; }
    public string DepartmentName { get; set; }
    public int CoursesCount { get; set; }
    public int TotalStudents { get; set; }
    public double AverageStudentsPerCourse { get; set; }
    public int TotalGrades { get; set; }
}

public class CourseEnrollmentDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    public string DepartmentName { get; set; }
    public string TeacherName { get; set; }
    public int EnrollmentCount { get; set; }
    public double AverageGrade { get; set; }
    public int MaxCapacity { get; set; }
    public double CapacityUtilization { get; set; }
}

public class SubjectPerformanceDto
{
    public string CourseName { get; set; }
    public int StudentCount { get; set; }
    public double AverageGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
    public int GradeCount { get; set; }
}

public class DepartmentOverviewDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int CourseCount { get; set; }
    public int EnrollmentCount { get; set; }
    public double AverageGrade { get; set; }
    public double PerformanceScore { get; set; }
}

// Activity statistics
public class ActivityStatsDto
{
    public int LoginsLast24Hours { get; set; }
    public int LoginsLast7Days { get; set; }
    public int LoginsLast30Days { get; set; }
    public int RegistrationsLast24Hours { get; set; }
    public int RegistrationsLast7Days { get; set; }
    public int RegistrationsLast30Days { get; set; }
    public int GradesSubmittedLast24Hours { get; set; }
    public int GradesSubmittedLast7Days { get; set; }
    public int GradesSubmittedLast30Days { get; set; }
    public int EnrollmentsLast24Hours { get; set; }
    public int EnrollmentsLast7Days { get; set; }
    public int EnrollmentsLast30Days { get; set; }
    public List<DailyActivityDto> DailyActivity { get; set; }
}

public class DailyActivityDto
{
    public DateTime Date { get; set; }
    public int Logins { get; set; }
    public int Registrations { get; set; }
    public int GradesSubmitted { get; set; }
    public int Enrollments { get; set; }
}

// System health statistics
public class SystemHealthDto
{
    public bool DatabaseHealthy { get; set; }
    public bool AuthenticationHealthy { get; set; }
    public bool FileStorageHealthy { get; set; }
    public long DatabaseSize { get; set; }
    public int ActiveConnections { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public List<string> Warnings { get; set; }
}

// Comprehensive dashboard response
public class ComprehensiveDashboardDto
{
    public DashboardStatsDto Overview { get; set; }
    public StudentStatsDto Students { get; set; }
    public TeacherStatsDto Teachers { get; set; }
    public CourseStatsDto Courses { get; set; }
    public GradeStatsDto Grades { get; set; }
    public DepartmentStatsDto Departments { get; set; }
    public ActivityStatsDto Activity { get; set; }
    public SystemHealthDto Health { get; set; }
    public DateTime GeneratedAt { get; set; }
    public TimeSpan GenerationTime { get; set; }
}
