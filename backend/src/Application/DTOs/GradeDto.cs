namespace UniversityManagement.Application.DTOs;

public class GradeDto
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public double Score { get; set; }
    public string? Remarks { get; set; }
}

public class GradeResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public double Score { get; set; }
    public string? Remarks { get; set; }
    public string StudentName { get; set; }
    public string StudentEmail { get; set; }
    public string CourseName { get; set; }
    public string DepartmentName { get; set; }
    public string TeacherName { get; set; }
    public DateTime GradeDate { get; set; }
    public string LetterGrade { get; set; }
}
