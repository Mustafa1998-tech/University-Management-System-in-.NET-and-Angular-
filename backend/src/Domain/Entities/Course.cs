namespace UniversityManagement.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public int TeacherId { get; set; }

    public Department Department { get; set; }
    public Teacher Teacher { get; set; }
    public ICollection<StudentCourse> StudentCourses { get; set; }
    public ICollection<Grade> Grades { get; set; }
}
