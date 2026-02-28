namespace UniversityManagement.Application.DTOs;

public class CourseDto
{
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public int? TeacherId { get; set; } // Optional teacher assignment
}

public class CourseResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public int? TeacherId { get; set; }
    public string DepartmentName { get; set; }
    public string TeacherName { get; set; }
    public int StudentCount { get; set; }
    public int EnrolledStudentsCount { get; set; }
    public double AverageGrade { get; set; }
    public string AverageLetterGrade { get; set; }
}
