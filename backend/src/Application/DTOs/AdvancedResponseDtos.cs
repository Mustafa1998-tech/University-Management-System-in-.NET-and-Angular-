namespace UniversityManagement.Application.DTOs;

// 🎓 Student Response DTO with full relations
public class StudentResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public int EnrolledCoursesCount { get; set; }
    public double AverageGrade { get; set; }
    public string AverageLetterGrade { get; set; }
}

// 👨‍🏫 Teacher Response DTO with full relations
public class TeacherResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public int CoursesCount { get; set; }
    public int TotalStudents { get; set; }
}

// 📘 Course Response DTO with full relations
public class CourseResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int? TeacherId { get; set; }
    public string TeacherName { get; set; }
    public int EnrolledStudentsCount { get; set; }
    public double AverageGrade { get; set; }
    public string AverageLetterGrade { get; set; }
}

// 📝 Enrollment Response DTO with full relations
public class EnrollmentResponseDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string StudentName { get; set; }
    public string StudentEmail { get; set; }
    public string CourseName { get; set; }
    public string DepartmentName { get; set; }
    public string TeacherName { get; set; }
    public DateTime EnrollmentDate { get; set; }
}

// 🧮 Grade Response DTO with full relations
public class GradeResponseDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public double Score { get; set; }
    public string LetterGrade { get; set; }
    public string StudentName { get; set; }
    public string StudentEmail { get; set; }
    public string CourseName { get; set; }
    public string DepartmentName { get; set; }
    public string TeacherName { get; set; }
    public DateTime GradeDate { get; set; }
    public string? Remarks { get; set; }
}

// 🏢 Department Response DTO with relations
public class DepartmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int StudentsCount { get; set; }
    public int TeachersCount { get; set; }
    public int CoursesCount { get; set; }
    public double AverageGrade { get; set; }
}

// 👤 User Response DTO
public class UserResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string FullName { get; set; }
    public string DepartmentName { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 📊 Statistics DTOs
public class StudentStatisticsDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; }
    public int EnrolledCourses { get; set; }
    public double AverageGrade { get; set; }
    public string AverageLetterGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
    public int TotalGrades { get; set; }
}

public class CourseStatisticsDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    public int EnrolledStudents { get; set; }
    public double AverageGrade { get; set; }
    public string AverageLetterGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
    public int TotalGrades { get; set; }
}

public class DepartmentStatisticsDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalCourses { get; set; }
    public double AverageGrade { get; set; }
    public int TotalEnrollments { get; set; }
}

// 🔍 Search/Filter DTOs
public class SearchParametersDto
{
    public string Query { get; set; }
    public int? DepartmentId { get; set; }
    public int? TeacherId { get; set; }
    public string Role { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

public class PaginatedResponseDto<T>
{
    public List<T> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
