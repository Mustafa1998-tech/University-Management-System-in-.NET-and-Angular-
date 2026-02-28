namespace UniversityManagement.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }

    public Student Student { get; set; }
}
