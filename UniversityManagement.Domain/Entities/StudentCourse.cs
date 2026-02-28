namespace UniversityManagement.Domain.Entities;

public class StudentCourse
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; }
    public Course Course { get; set; }
}
