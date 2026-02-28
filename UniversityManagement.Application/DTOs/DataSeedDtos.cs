namespace UniversityManagement.Application.DTOs;

public class HistoricalDataSeedRequestDto
{
    public int Years { get; set; } = 5;
    public int DepartmentsCount { get; set; } = 8;
    public int TeachersPerDepartment { get; set; } = 80;
    public int CoursesPerDepartmentPerYear { get; set; } = 15;
    public int StudentsPerYear { get; set; } = 3500;
    public int MinEnrollmentsPerStudent { get; set; } = 4;
    public int MaxEnrollmentsPerStudent { get; set; } = 7;
    public double GradeCoverageRate { get; set; } = 0.92;
    public bool GeneratePayments { get; set; } = true;
    public int PaymentsPerStudentPerYear { get; set; } = 2;
    public int BatchSize { get; set; } = 2000;
    public bool ClearPreviousSeedData { get; set; } = false;
    public string DefaultPassword { get; set; } = "Seed@12345";
    public int? RandomSeed { get; set; }
    public string? RunTag { get; set; }
}

public class HistoricalDataSeedResultDto
{
    public string RunTag { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public long DurationMs { get; set; }
    public string DefaultPassword { get; set; } = string.Empty;

    public int DepartmentsCreated { get; set; }
    public int TeacherUsersCreated { get; set; }
    public int StudentUsersCreated { get; set; }
    public int TeachersCreated { get; set; }
    public int StudentsCreated { get; set; }
    public int CoursesCreated { get; set; }
    public int EnrollmentsCreated { get; set; }
    public int GradesCreated { get; set; }
    public int PaymentsCreated { get; set; }

    public int DeletedUsers { get; set; }
    public int DeletedTeachers { get; set; }
    public int DeletedStudents { get; set; }
    public int DeletedCourses { get; set; }
    public int DeletedEnrollments { get; set; }
    public int DeletedGrades { get; set; }
    public int DeletedPayments { get; set; }

    public int TotalUsersCreated => TeacherUsersCreated + StudentUsersCreated;
}
