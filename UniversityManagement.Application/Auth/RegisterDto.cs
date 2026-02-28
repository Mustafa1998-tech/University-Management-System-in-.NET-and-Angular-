namespace UniversityManagement.Application.Auth;

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; } // Admin, Student, Teacher, Accountant
    public string FullName { get; set; }
    public int DepartmentId { get; set; }
}
