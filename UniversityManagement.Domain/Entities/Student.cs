namespace UniversityManagement.Domain.Entities;

public class Student
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; }
    public int DepartmentId { get; set; }

    public User User { get; set; }
    public Department Department { get; set; }
    public ICollection<StudentCourse> StudentCourses { get; set; }
    public ICollection<Grade> Grades { get; set; }
    public ICollection<Payment> Payments { get; set; }
}
