namespace UniversityManagement.Application.DTOs;

public class EnrollmentDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
}

public class EnrollmentResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string StudentName { get; set; }
    public string StudentEmail { get; set; }
    public string CourseName { get; set; }
    public string DepartmentName { get; set; }
    public string TeacherName { get; set; }
    public DateTime EnrollmentDate { get; set; }
}
