namespace UniversityManagement.Domain.Entities;

public class Teacher
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; }
    public int DepartmentId { get; set; }

    public User User { get; set; }
    public Department Department { get; set; }
    public ICollection<Course> Courses { get; set; }
}
