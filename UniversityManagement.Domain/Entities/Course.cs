namespace UniversityManagement.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public int? TeacherId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Department Department { get; set; }
    public Teacher? Teacher { get; set; }
    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();
}
