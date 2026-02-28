namespace UniversityManagement.Application.DTOs;

public class StudentDto
{
    public string FullName { get; set; }
    public int DepartmentId { get; set; }
    public int UserId { get; set; }
}

public class StudentResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public int DepartmentId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public string DepartmentName { get; set; }
    public int EnrolledCoursesCount { get; set; }
    public double AverageGrade { get; set; }
    public string AverageLetterGrade { get; set; }
}
