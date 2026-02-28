namespace UniversityManagement.Domain.Entities;

public class Grade
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public double Value { get; set; }

    public Student Student { get; set; }
    public Course Course { get; set; }
}
