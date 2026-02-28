namespace UniversityManagement.Application.DTOs;

public class TeacherDto
{
    public string FullName { get; set; }
    public int DepartmentId { get; set; }
    public int UserId { get; set; }
}

public class TeacherResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public int DepartmentId { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; }
    public string DepartmentName { get; set; }
}
