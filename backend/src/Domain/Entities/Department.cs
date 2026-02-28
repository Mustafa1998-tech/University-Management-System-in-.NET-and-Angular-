namespace UniversityManagement.Domain.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Student> Students { get; set; }
    public ICollection<Teacher> Teachers { get; set; }
    public ICollection<Course> Courses { get; set; }
}
